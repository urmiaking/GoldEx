using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoicePaymentService(
    IInvoicePaymentRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    IServerLedgerAccountService ledgerAccountService)
    : IServerInvoicePaymentService
{
    public async Task SyncPaymentsWithInvoiceAsync(Invoice invoice, List<InvoicePaymentDto> invoicePayments,
        CancellationToken cancellationToken = default)
    {
        var existingPayments = await repository
            .Get(new InvoicePaymentsByInvoiceIdSpecification(invoice.Id))
            .ToListAsync(cancellationToken);

        var paymentsToCreate = new List<InvoicePayment>();
        var paymentsToUpdate = new List<InvoicePayment>();

        var dtoById = invoicePayments
            .Where(p => p.Id.HasValue)
            .ToDictionary(p => p.Id!.Value, p => p);

        var paymentsToDelete = existingPayments.Where(existing => !dtoById.ContainsKey(existing.Id.Value)).ToList();

        foreach (var paymentDto in invoicePayments)
        {
            LedgerAccountId? ledgerAccountId = null;
            LedgerAccount? ledgerAccount = null;
            FinancialAccountId? financialAccountId = null;
            var priceUnitId = new PriceUnitId(paymentDto.PriceUnitId);

            switch (paymentDto.PaymentType)
            {
                case PaymentType.InternalCash:
                    if (!paymentDto.FinancialAccountId.HasValue)
                        throw new InvalidOperationException("FinancialAccountId is required for cash payments.");
                    financialAccountId = new FinancialAccountId(paymentDto.FinancialAccountId.Value);
                    break;

                case PaymentType.MoltenGoldInventory:
                    var moltenGoldAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
                        .FirstOrDefaultAsync(cancellationToken);
                    ledgerAccountId = moltenGoldAccount?.Id
                        ?? throw new NotFoundException("Molten Gold Inventory ledger account not found.");
                    break;

                case PaymentType.UsedGoldInventory:
                    var usedGoldAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.UsedProductInventory))
                        .FirstOrDefaultAsync(cancellationToken);
                    ledgerAccountId = usedGoldAccount?.Id
                        ?? throw new NotFoundException("Used Product Inventory ledger account not found.");
                    break;

                case PaymentType.CustomerTransfer:
                    if (!paymentDto.CustomerId.HasValue)
                        throw new NotFoundException("CustomerId is required for customer transfer payments.");

                    var role = invoice.InvoiceType is InvoiceType.Sell
                        ? LedgerAccountRole.Payable
                        : LedgerAccountRole.Receivable;

                    var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                        new CustomerId(paymentDto.CustomerId.Value),
                        priceUnitId,
                        role,
                        cancellationToken
                    );

                    ledgerAccountId = customerLedger.Id;
                    ledgerAccount = customerLedger;
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported payment type: {paymentDto.PaymentType}");
            }

            if (!paymentDto.Id.HasValue)
            {
                var newPayment = InvoicePayment.Create(
                    paymentDto.PaymentDate,
                    paymentDto.PaymentType,
                    paymentDto.PaymentSide,
                    paymentDto.Amount,
                    paymentDto.ExchangeRate,
                    paymentDto.GoldFineness,
                    invoice.Id,
                    priceUnitId,
                    financialAccountId,
                    ledgerAccountId,
                    paymentDto.VoucherId.HasValue ? new PaymentVoucherId(paymentDto.VoucherId.Value) : null,
                    paymentDto.ReferenceNumber,
                    paymentDto.Note
                );

                newPayment.SetLedgerAccount(ledgerAccount);
                
                paymentsToCreate.Add(newPayment);
            }
            else
            {
                var existingPayment = existingPayments.FirstOrDefault(p => p.Id.Value == paymentDto.Id.Value);
                if (existingPayment is null)
                    throw new NotFoundException($"Payment with Id {paymentDto.Id} not found for update.");

                existingPayment.SetAmount(paymentDto.Amount, new PriceUnitId(paymentDto.PriceUnitId));
                existingPayment.SetPaymentType(paymentDto.PaymentType);
                existingPayment.SetPaymentSide(paymentDto.PaymentSide);
                existingPayment.SetExchangeRate(paymentDto.ExchangeRate);
                existingPayment.SetLedgerAccountId(ledgerAccountId);
                existingPayment.SetLedgerAccount(ledgerAccount);
                existingPayment.SetSourceFinancialAccountId(financialAccountId);
                existingPayment.SetNote(paymentDto.Note);
                existingPayment.SetReferenceNumber(paymentDto.ReferenceNumber);
                existingPayment.SetPaymentDate(paymentDto.PaymentDate);
                existingPayment.SetFinalAmount(paymentDto.Amount, paymentDto.GoldFineness);
                existingPayment.SetPaymentVoucherId(paymentDto.VoucherId.HasValue ? new PaymentVoucherId(paymentDto.VoucherId.Value) : null);

                paymentsToUpdate.Add(existingPayment);
            }
        }

        if (paymentsToDelete.Any())
            repository.DeleteRange(paymentsToDelete);

        if (paymentsToCreate.Any())
            repository.CreateRange(paymentsToCreate);

        if (paymentsToUpdate.Any())
            repository.UpdateRange(paymentsToUpdate);

        var finalPayments = existingPayments
            .Except(paymentsToDelete) 
            .Union(paymentsToCreate)
            .ToList();

        invoice.SetPayments(finalPayments);

        await repository.SaveAsync(cancellationToken);
    }
}