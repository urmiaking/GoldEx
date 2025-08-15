using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.PaymentVouchers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PaymentVoucherService(
    IPaymentVoucherRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    ICustomerRepository customerRepository,
    IAccountingTransactionService transactionService,
    IMapper mapper,
    ILogger<PaymentVoucherService> logger,
    PaymentVoucherRequestDtoValidator validator,
    DeletePaymentVoucherValidator deleteValidator) : IPaymentVoucherService
{
    public async Task<PagedList<GetPaymentVoucherListResponse>> GetListAsync(RequestFilter filter, PaymentVoucherFilter voucherFilter, Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new PaymentVouchersByFilterSpecification(
            filter,
            voucherFilter,
            customerId.HasValue ? new CustomerId(customerId.Value) : null
        );

        var data = await repository
            .Get(spec)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetPaymentVoucherListResponse>
        {
            Data = mapper.Map<List<GetPaymentVoucherListResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<List<GetPaymentVoucherResponse>> GetPendingListAsync(Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new PaymentVouchersByCustomerIdSpecification(new CustomerId(customerId)))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPaymentVoucherResponse>>(list);
    }

    public async Task<GetPaymentVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPaymentVoucherResponse>(item);
    }

    public async Task<GetPaymentVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByNumberSpecification(voucherNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPaymentVoucherResponse>(item);
    }

    public async Task CreateAsync(PaymentVoucherRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                #region Ledger Account

                var parentAccountTitle = request.VoucherType switch
                {
                    PaymentVoucherType.PrepaymentToSupplier => SystemLedgerAccounts.AccountsPayable,
                    PaymentVoucherType.RefundToCustomer => SystemLedgerAccounts.AccountsReceivable,
                    _ => throw new InvalidOperationException("Unsupported voucher type.")
                };

                var parentLedgerAccount = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(parentAccountTitle))
                                              .FirstOrDefaultAsync(cancellationToken)
                                          ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

                var customer = await customerRepository.Get(new CustomersByFinancialAccountIdSpecification(
                                       new FinancialAccountId(request.DestinationFinancialAccountId)))
                                   .FirstOrDefaultAsync(cancellationToken) ??
                               throw new NotFoundException(
                                   $"Customer with financial account {request.DestinationFinancialAccountId} not found.");

                var existingLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentSpecification(customer.Id, parentLedgerAccount.Id))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingLedgerAccount is null)
                {
                    var ledgerAccountType = parentLedgerAccount.AccountType;

                    var ledgerAccountTitle = $"{parentLedgerAccount.Title} - {customer.FullName}";
                    var newLedgerAccount = LedgerAccount.CreateCustomerAccount(
                        ledgerAccountTitle,
                        customer.Id,
                        ledgerAccountType,
                        parentLedgerAccount.Id);

                    await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
                }

                #endregion

                var paymentVoucher = PaymentVoucher.Create(
                    request.Amount,
                    request.VoucherNumber,
                    request.Description,
                    request.ExchangeRate,
                    request.PaymentDate,
                    request.VoucherType,
                    new FinancialAccountId(request.SourceFinancialAccountId),
                    new FinancialAccountId(request.DestinationFinancialAccountId),
                    new PriceUnitId(request.VoucherPriceUnitId)
                );

                await repository.CreateAsync(paymentVoucher, cancellationToken);

                await transactionService.CreateTransactionsForPaymentVoucherAsync(paymentVoucher, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task UpdateAsync(Guid id, PaymentVoucherRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        #region Ledger Account

        var parentAccountTitle = request.VoucherType switch
        {
            PaymentVoucherType.PrepaymentToSupplier => SystemLedgerAccounts.AccountsPayable,
            PaymentVoucherType.RefundToCustomer => SystemLedgerAccounts.AccountsReceivable,
            _ => throw new InvalidOperationException("Unsupported voucher type.")
        };

        var parentLedgerAccount = await ledgerAccountRepository
                                      .Get(new LedgerAccountsByTitleSpecification(parentAccountTitle))
                                      .FirstOrDefaultAsync(cancellationToken)
                                  ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

        var customer = await customerRepository.Get(new CustomersByFinancialAccountIdSpecification(
                               new FinancialAccountId(request.DestinationFinancialAccountId)))
                           .FirstOrDefaultAsync(cancellationToken) ??
                       throw new NotFoundException(
                           $"Customer with financial account {request.DestinationFinancialAccountId} not found.");

        var existingLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByCustomerAndParentSpecification(customer.Id, parentLedgerAccount.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingLedgerAccount is null)
        {
            var ledgerAccountType = parentLedgerAccount.AccountType;

            var ledgerAccountTitle = $"{parentLedgerAccount.Title} - {customer.FullName}";
            var newLedgerAccount = LedgerAccount.CreateCustomerAccount(
                ledgerAccountTitle,
                customer.Id,
                ledgerAccountType,
                parentLedgerAccount.Id);

            await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
        }

        #endregion

        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetAmount(request.Amount);
        item.SetVoucherNumber(request.VoucherNumber);
        item.SetDescription(request.Description);
        item.SetPaymentDate(request.PaymentDate);
        item.SetSourceFinancialAccountId(new FinancialAccountId(request.SourceFinancialAccountId));
        item.SetDestinationFinancialAccountId(new FinancialAccountId(request.DestinationFinancialAccountId));
        item.SetVoucherPriceUnitId(new PriceUnitId(request.VoucherPriceUnitId));
        item.SetExchangeRate(request.ExchangeRate);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

        await transactionService.ClearTransactionsForPaymentVoucherAsync(item, cancellationToken);

        await repository.DeleteAsync(item, cancellationToken);
    }

    public async Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await repository.GetLastNumberAsync(cancellationToken);
        return new GetVoucherNumberResponse(lastNumber);
    }
}