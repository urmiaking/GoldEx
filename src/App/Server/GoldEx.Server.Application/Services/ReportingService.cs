using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(
    ITransactionRepository transactionRepository,
    IInvoiceRepository invoiceRepository,
    IInvoicePaymentRepository paymentRepository,
    IMapper mapper) : IReportingService
{
    public async Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var models = await transactionRepository.GetLedgerAccountStatementsAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountStatementRpResponse>>(models);
    }

    public async Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountTrialBalanceRpResponse>>(list.Nodes);
    }

    public async Task<List<CustomerRemainingBalanceRpResponse>> GetCustomerRemainingBalanceAsync(CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerRemainingBalanceAsync(request, cancellationToken);
        return mapper.Map<List<CustomerRemainingBalanceRpResponse>>(list);
    }

    public async Task<List<CustomerTransactionRpResponse>> GetCustomerTransactionsAsync(CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerTransactionsAsync(request, cancellationToken);
        return mapper.Map<List<CustomerTransactionRpResponse>>(list);
    }

    public async Task<List<SellInvoiceRpResponse>> GetSellInvoicesAsync(SellInvoiceRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await invoiceRepository
            .Get(new InvoicesReportSpecification(InvoiceType.Sell,
                request.PaymentStatus,
                request.PriceUnitId,
                request.CustomerId,
                request.FromDate,
                request.ToDate))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SellInvoiceRpResponse>>(list);
    }

    public async Task<List<PurchaseInvoiceRpResponse>> GetPurchaseInvoicesAsync(PurchaseInvoiceRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await invoiceRepository
            .Get(new InvoicesReportSpecification(InvoiceType.Purchase,
                request.PaymentStatus,
                request.PriceUnitId,
                request.CustomerId,
                request.FromDate,
                request.ToDate))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<PurchaseInvoiceRpResponse>>(list);
    }

    public async Task<List<PaymentRpResponse>> GetPaymentsAsync(PaymentRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await paymentRepository
            .Get(new InvoicePaymentsByReportSpecification(request))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<PaymentRpResponse>>(list);
    }

    public async Task<List<InvoicePaymentRpResponse>> GetInvoicePaymentsAsync(InvoicePaymentRpRequest request, CancellationToken cancellationToken = default)
    {
        var list = await paymentRepository
            .Get(new InvoicePaymentsByNumberSpecification(request.InvoiceNumber, request.InvoiceType))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<InvoicePaymentRpResponse>>(list);
    }
}