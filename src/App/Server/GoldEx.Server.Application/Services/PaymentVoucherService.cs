using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.PaymentVouchers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PaymentVoucherService(
    IPaymentVoucherRepository repository,
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

                var paymentVoucher = PaymentVoucher.Create(
                    request.Amount,
                    request.VoucherNumber,
                    request.Description,
                    request.ExchangeRate,
                    request.PaymentDate,
                    request.VoucherType,
                    new CustomerId(request.CustomerId),
                    new FinancialAccountId(request.SourceFinancialAccountId),
                    request.DestinationFinancialAccountId.HasValue ? new FinancialAccountId(request.DestinationFinancialAccountId.Value) : null,
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
        // ۱. شروع تراکنش دیتابیس برای تضمین یکپارچگی
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var paymentVoucher = await repository
                    .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Payment voucher not found.");

                paymentVoucher.SetAmount(request.Amount);
                paymentVoucher.SetCustomerId(new CustomerId(request.CustomerId));
                paymentVoucher.SetVoucherNumber(request.VoucherNumber);
                paymentVoucher.SetDescription(request.Description);
                paymentVoucher.SetPaymentDate(request.PaymentDate);
                paymentVoucher.SetVoucherType(request.VoucherType);
                paymentVoucher.SetSourceFinancialAccountId(new FinancialAccountId(request.SourceFinancialAccountId));
                paymentVoucher.SetDestinationFinancialAccountId(request.DestinationFinancialAccountId.HasValue
                    ? new FinancialAccountId(request.DestinationFinancialAccountId.Value)
                    : null);
                paymentVoucher.SetVoucherPriceUnitId(new PriceUnitId(request.VoucherPriceUnitId));
                paymentVoucher.SetExchangeRate(request.ExchangeRate);

                await repository.UpdateAsync(paymentVoucher, cancellationToken);

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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var item = await repository
                    .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

                await transactionService.ClearTransactionsForPaymentVoucherAsync(item, cancellationToken);

                await repository.DeleteAsync(item, cancellationToken);

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

    public async Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await repository.GetLastNumberAsync(cancellationToken);
        return new GetVoucherNumberResponse(lastNumber);
    }
}