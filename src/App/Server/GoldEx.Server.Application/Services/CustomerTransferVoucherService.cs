using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.CustomerTransfers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.CustomerTransfers;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CustomerTransferVoucherService(
    ICustomerTransferVoucherRepository repository,
    IInvoicePaymentRepository invoicePaymentRepository,
    IInvoiceRepository invoiceRepository,
    ICustomerRepository customerRepository,
    IServerLedgerAccountService ledgerAccountService,
    IAccountingTransactionService transactionService,
    ILogger<CustomerTransferVoucherService> logger,
    CreateCustomerTransferVoucherRequestValidator createValidator,
    UpdateCustomerTransferVoucherRequestValidator updateValidator) : ICustomerTransferVoucherService
{
    public async Task<PagedList<GetCustomerTransferVoucherListResponse>> GetListAsync(
        RequestFilter filter,
        CustomerTransferVoucherFilter voucherFilter,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new CustomerTransferVouchersByFilterSpecification(filter, voucherFilter);

        var data = await repository
            .Get(spec)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(spec, cancellationToken);

        var responses = data.Select(v => new GetCustomerTransferVoucherListResponse
        {
            Id = v.Id.Value,
            VoucherNumber = v.VoucherNumber,
            TransferDate = v.TransferDate,
            SourceCustomerId = v.SourceCustomerId.Value,
            SourceCustomerName = v.SourceCustomer?.FullName ?? string.Empty,
            DestinationCustomerId = v.DestinationCustomerId.Value,
            DestinationCustomerName = v.DestinationCustomer?.FullName ?? string.Empty,
            PriceUnitId = v.PriceUnitId.Value,
            PriceUnitTitle = v.PriceUnit?.Title ?? string.Empty,
            PriceUnitIsGoldBased = v.PriceUnit?.IsGoldBased ?? false,
            Amount = v.Amount,
            ExchangeRate = v.ExchangeRate,
            SourceInvoiceId = v.SourceInvoiceId?.Value,
            SourceInvoiceNumber = v.SourceInvoice?.InvoiceNumber,
            DestinationInvoiceId = v.DestinationInvoiceId?.Value,
            DestinationInvoiceNumber = v.DestinationInvoice?.InvoiceNumber,
            Description = v.Description,
            CreatedAt = v.CreatedAt
        }).ToList();

        return new PagedList<GetCustomerTransferVoucherListResponse>
        {
            Data = responses,
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetCustomerTransferVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomerTransferVouchersByIdSpecification(new CustomerTransferVoucherId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("سند حواله یافت نشد.");

        return MapToResponse(item);
    }

    public async Task<GetCustomerTransferVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomerTransferVouchersByNumberSpecification(voucherNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("سند حواله یافت نشد.");

        return MapToResponse(item);
    }

    public async Task CreateAsync(CreateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var lastNumber = await repository.GetLastNumberAsync(cancellationToken);
            var nextNumber = lastNumber + 1;

            var voucher = CustomerTransferVoucher.Create(
                nextNumber,
                request.TransferDate,
                new CustomerId(request.SourceCustomerId),
                new CustomerId(request.DestinationCustomerId),
                new PriceUnitId(request.PriceUnitId),
                request.Amount,
                request.ExchangeRate,
                request.SourceInvoiceId.HasValue ? new InvoiceId(request.SourceInvoiceId.Value) : null,
                request.DestinationInvoiceId.HasValue ? new InvoiceId(request.DestinationInvoiceId.Value) : null,
                request.Description
            );

            await repository.CreateAsync(voucher, cancellationToken);

            // Handle optional Linked Invoice Payments for settlement
            await ApplyInvoicePaymentsAsync(voucher, cancellationToken);

            // Create Accounting Transactions
            await transactionService.CreateTransactionsForCustomerTransferVoucherAsync(voucher, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var voucher = await repository
                .Get(new CustomerTransferVouchersByIdSpecification(new CustomerTransferVoucherId(id)))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("سند حواله یافت نشد.");

            voucher.SetTransferDate(request.TransferDate);
            voucher.SetSourceCustomerId(new CustomerId(request.SourceCustomerId));
            voucher.SetDestinationCustomerId(new CustomerId(request.DestinationCustomerId));
            voucher.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));
            voucher.SetAmount(request.Amount);
            voucher.SetExchangeRate(request.ExchangeRate);
            voucher.SetSourceInvoiceId(request.SourceInvoiceId.HasValue ? new InvoiceId(request.SourceInvoiceId.Value) : null);
            voucher.SetDestinationInvoiceId(request.DestinationInvoiceId.HasValue ? new InvoiceId(request.DestinationInvoiceId.Value) : null);
            voucher.SetDescription(request.Description);

            await repository.UpdateAsync(voucher, cancellationToken);

            // Clear old linked invoice payments & re-apply
            await ClearInvoicePaymentsAsync(voucher.Id, cancellationToken);
            await ApplyInvoicePaymentsAsync(voucher, cancellationToken);

            // Update Accounting Transactions
            await transactionService.CreateTransactionsForCustomerTransferVoucherAsync(voucher, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var voucherId = new CustomerTransferVoucherId(id);
            var item = await repository
                .Get(new CustomerTransferVouchersByIdSpecification(voucherId))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("سند حواله یافت نشد.");

            await ClearInvoicePaymentsAsync(voucherId, cancellationToken);
            await transactionService.ClearTransactionsForCustomerTransferVoucherAsync(item, cancellationToken);

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

    public async Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await repository.GetLastNumberAsync(cancellationToken);
        return new GetVoucherNumberResponse(lastNumber);
    }

    private static GetCustomerTransferVoucherResponse MapToResponse(CustomerTransferVoucher v) => new()
    {
        Id = v.Id.Value,
        VoucherNumber = v.VoucherNumber,
        TransferDate = v.TransferDate,
        SourceCustomerId = v.SourceCustomerId.Value,
        SourceCustomerName = v.SourceCustomer?.FullName ?? string.Empty,
        DestinationCustomerId = v.DestinationCustomerId.Value,
        DestinationCustomerName = v.DestinationCustomer?.FullName ?? string.Empty,
        PriceUnitId = v.PriceUnitId.Value,
        PriceUnitTitle = v.PriceUnit?.Title ?? string.Empty,
        PriceUnitIsGoldBased = v.PriceUnit?.IsGoldBased ?? false,
        Amount = v.Amount,
        ExchangeRate = v.ExchangeRate,
        SourceInvoiceId = v.SourceInvoiceId?.Value,
        SourceInvoiceNumber = v.SourceInvoice?.InvoiceNumber,
        DestinationInvoiceId = v.DestinationInvoiceId?.Value,
        DestinationInvoiceNumber = v.DestinationInvoice?.InvoiceNumber,
        Description = v.Description,
        CreatedAt = v.CreatedAt
    };

    private async Task ApplyInvoicePaymentsAsync(CustomerTransferVoucher voucher, CancellationToken cancellationToken)
    {
        var dateTime = voucher.TransferDate.ToDateTime(TimeOnly.FromTimeSpan(voucher.CreatedAt.TimeOfDay));

        var sourceCustomer = await customerRepository
            .Get(new CustomersByIdSpecification(voucher.SourceCustomerId))
            .FirstOrDefaultAsync(cancellationToken);

        var destCustomer = await customerRepository
            .Get(new CustomersByIdSpecification(voucher.DestinationCustomerId))
            .FirstOrDefaultAsync(cancellationToken);

        var destLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
            voucher.DestinationCustomerId, voucher.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

        var sourceLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
            voucher.SourceCustomerId, voucher.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

        // Source Invoice Payment (e.g. Ali's Sales Invoice)
        if (voucher.SourceInvoiceId.HasValue)
        {
            var sourceInvoice = await invoiceRepository
                .Get(new InvoicesByIdSpecification(voucher.SourceInvoiceId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceInvoice != null)
            {
                var side = sourceInvoice.InvoiceType == InvoiceType.Sell ? PaymentSide.Receive : PaymentSide.Pay;
                var destName = destCustomer?.FullName ?? string.Empty;
                var note = $"حواله شماره {voucher.VoucherNumber} به حساب {destName}".Trim();

                var payment = InvoicePayment.Create(
                    dateTime,
                    PaymentType.CustomerTransfer,
                    side,
                    voucher.Amount,
                    voucher.ExchangeRate,
                    null,
                    sourceInvoice.Id,
                    voucher.PriceUnitId,
                    null,
                    destLedger.Id,
                    null,
                    null,
                    voucher.DestinationInvoiceId,
                    voucher.VoucherNumber.ToString(),
                    note,
                    voucher.StoreId,
                    voucher.Id
                );

                await invoicePaymentRepository.CreateAsync(payment, cancellationToken);
            }
        }

        // Destination Invoice Payment (e.g. Masoud's Purchase Invoice)
        if (voucher.DestinationInvoiceId.HasValue)
        {
            var destInvoice = await invoiceRepository
                .Get(new InvoicesByIdSpecification(voucher.DestinationInvoiceId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (destInvoice != null)
            {
                var side = destInvoice.InvoiceType == InvoiceType.Purchase ? PaymentSide.Pay : PaymentSide.Receive;
                var sourceName = sourceCustomer?.FullName ?? string.Empty;
                var note = $"حواله شده از حساب {sourceName} طبق سند شماره {voucher.VoucherNumber}".Trim();

                var payment = InvoicePayment.Create(
                    dateTime,
                    PaymentType.TransferedPayment,
                    side,
                    voucher.Amount,
                    voucher.ExchangeRate,
                    null,
                    destInvoice.Id,
                    voucher.PriceUnitId,
                    null,
                    sourceLedger.Id,
                    null,
                    null,
                    voucher.SourceInvoiceId,
                    voucher.VoucherNumber.ToString(),
                    note,
                    voucher.StoreId,
                    voucher.Id
                );

                await invoicePaymentRepository.CreateAsync(payment, cancellationToken);
            }
        }
    }

    private async Task ClearInvoicePaymentsAsync(CustomerTransferVoucherId voucherId, CancellationToken cancellationToken)
    {
        var existingPayments = await invoicePaymentRepository
            .Get(new InvoicePaymentsByCustomerTransferVoucherIdSpecification(voucherId))
            .ToListAsync(cancellationToken);

        if (existingPayments.Count > 0)
        {
            var sourceIds = existingPayments.Select(p => p.Id).ToList();
            var childPayments = await invoicePaymentRepository
                .Get(new InvoicePaymentsBySourcePaymentIdsSpecification(sourceIds))
                .ToListAsync(cancellationToken);

            if (childPayments.Count > 0)
            {
                await invoicePaymentRepository.DeleteRangeAsync(childPayments, cancellationToken);
            }

            await invoicePaymentRepository.DeleteRangeAsync(existingPayments, cancellationToken);
        }
    }
}

internal class InvoicePaymentsByCustomerTransferVoucherIdSpecification : GoldEx.Sdk.Server.Infrastructure.Specifications.SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsByCustomerTransferVoucherIdSpecification(CustomerTransferVoucherId voucherId)
    {
        AddCriteria(x => x.CustomerTransferVoucherId == voucherId);
    }
}
