using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.CheckPayments;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoicePaymentService(
    IInvoicePaymentRepository repository,
    IInvoicePaymentRepository paymentRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    ICheckPaymentRepository checkPaymentRepository,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    IServerLedgerAccountService ledgerAccountService)
    : IServerInvoicePaymentService
{
    public async Task SyncPaymentsWithInvoiceAsync(Invoice invoice, List<InvoicePaymentDto> invoicePayments,
        CancellationToken cancellationToken = default)
    {
        var existingPayments = await repository
            .Get(new InvoicePaymentsByInvoiceIdSpecification(invoice.Id))
            .Include(x => x.Invoice!.Customer)
            .ToListAsync(cancellationToken);

        var paymentsToCreate = new List<InvoicePayment>();
        var paymentsToUpdate = new List<InvoicePayment>();
        var checkPaymentsToCreate = new List<(InvoicePayment Payment, InvoicePaymentDto Dto)>();
        var checkPaymentsToUpdate = new List<(InvoicePayment Payment, InvoicePaymentDto Dto)>();

        var dtoById = invoicePayments
            .Where(p => p.Id.HasValue)
            .ToDictionary(p => p.Id!.Value, p => p);

        var paymentsToDelete = existingPayments.Where(existing => !dtoById.ContainsKey(existing.Id.Value)).ToList();

        var sourcePaymentsToDelete = paymentsToDelete
            .Where(p =>
                p.PaymentType == PaymentType.CustomerTransfer &&
                p.SourcePaymentId == null)
            .ToList();

        if (sourcePaymentsToDelete.Any())
        {
            var sourceIds = sourcePaymentsToDelete
                .Select(p => p.Id)
                .ToList();

            var targetPayments = await paymentRepository
                .Get(new InvoicePaymentsBySourcePaymentIdsSpecification(sourceIds))
                .ToListAsync(cancellationToken);

            if (targetPayments.Any())
                paymentRepository.DeleteRange(targetPayments);
        }

        foreach (var paymentDto in invoicePayments)
        {
            LedgerAccountId? ledgerAccountId = null;
            LedgerAccount? ledgerAccount = null;
            FinancialAccountId? financialAccountId = null;
            InvoiceId? targetInvoiceId = null;
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

                    if (paymentDto.TargetInvoiceId.HasValue) 
                        targetInvoiceId = new InvoiceId(paymentDto.TargetInvoiceId.Value);

                    break;

                case PaymentType.Check:
                    if (!paymentDto.CheckIssuerId.HasValue)
                        throw new NotFoundException("CheckIssuer is required for check payments");

                    if (!paymentDto.CheckIssuerFinancialAccountId.HasValue)
                        throw new NotFoundException("CheckIssuerFinancialAccountId is required for check payments");

                    // Later, after saving InvoicePayment, we will create CheckPayment
                    break;
                case PaymentType.TransferedPayment:
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
                    null,
                    paymentDto.TargetInvoiceId.HasValue ? new InvoiceId(paymentDto.TargetInvoiceId.Value) : null,
                    paymentDto.ReferenceNumber,
                    paymentDto.Note
                );

                newPayment.SetInvoice(invoice);

                newPayment.SetLedgerAccount(ledgerAccount);

                paymentsToCreate.Add(newPayment);

                if (paymentDto.PaymentType == PaymentType.Check) 
                    checkPaymentsToCreate.Add((newPayment, paymentDto));

                if (paymentDto.TargetInvoiceId.HasValue)
                    await UpdateTargetInvoicePaymentsAsync(newPayment, cancellationToken);
            }
            else
            {
                var existingPayment = existingPayments.FirstOrDefault(p => p.Id.Value == paymentDto.Id.Value);
                if (existingPayment is null)
                    throw new NotFoundException($"Payment with Id {paymentDto.Id} not found for update.");

                if (existingPayment.PaymentType is PaymentType.TransferedPayment)
                {
                    // No need to update
                    continue;
                }

                existingPayment.SetAmount(paymentDto.Amount, new PriceUnitId(paymentDto.PriceUnitId));
                existingPayment.SetPaymentType(paymentDto.PaymentType);
                existingPayment.SetPaymentSide(paymentDto.PaymentSide);
                existingPayment.SetExchangeRate(paymentDto.ExchangeRate);
                existingPayment.SetLedgerAccountId(ledgerAccountId);
                existingPayment.SetLedgerAccount(ledgerAccount);
                existingPayment.SetSourceFinancialAccountId(financialAccountId);
                existingPayment.SetTargetInvoiceId(targetInvoiceId);
                existingPayment.SetNote(paymentDto.Note);
                existingPayment.SetReferenceNumber(paymentDto.ReferenceNumber);
                existingPayment.SetPaymentDate(paymentDto.PaymentDate);
                existingPayment.SetFinalAmount(paymentDto.Amount, paymentDto.GoldFineness);
                existingPayment.SetPaymentVoucherId(paymentDto.VoucherId.HasValue ? new PaymentVoucherId(paymentDto.VoucherId.Value) : null);

                paymentsToUpdate.Add(existingPayment);

                if (paymentDto.PaymentType == PaymentType.Check)
                    checkPaymentsToUpdate.Add((existingPayment, paymentDto));

                if (paymentDto.TargetInvoiceId.HasValue)
                    await UpdateTargetInvoicePaymentsAsync(existingPayment, cancellationToken);
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

        foreach (var (payment, dto) in checkPaymentsToCreate)
        {
            var check = CheckPayment.Create(
                payment.Id,
                new CustomerId(dto.CheckIssuerId!.Value),
                new FinancialAccountId(dto.CheckIssuerFinancialAccountId!.Value),
                dto.CheckNumber,
                dto.CheckSayadiCode,
                dto.CheckDueDate!.Value
            );

            await checkPaymentRepository.CreateAsync(check, cancellationToken);

            if (dto.CheckImage != null && !string.IsNullOrEmpty(dto.CheckImageContentType))
            {
                await fileService.SaveLocalFileAsync(
                    webHostEnvironment.GetCheckPaymentFilePath(payment.Id.Value, dto.CheckImageContentType), dto.CheckImage,
                    cancellationToken);
            }
        }

        foreach (var (payment, dto) in checkPaymentsToUpdate)
        {
            var existingCheck = await checkPaymentRepository
                .Get(new CheckPaymentsByPaymentIdSpecification(payment.Id))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingCheck is null)
            {
                var newCheck = CheckPayment.Create(
                    payment.Id,
                    new CustomerId(dto.CheckIssuerId!.Value),
                    new FinancialAccountId(dto.CheckIssuerFinancialAccountId!.Value),
                    dto.CheckNumber,
                    dto.CheckSayadiCode,
                    dto.CheckDueDate!.Value
                );

                await checkPaymentRepository.CreateAsync(newCheck, cancellationToken);

                if (dto.CheckImage != null && !string.IsNullOrEmpty(dto.CheckImageContentType))
                {
                    await fileService.SaveLocalFileAsync(
                        webHostEnvironment.GetCheckPaymentFilePath(payment.Id.Value, dto.CheckImageContentType), dto.CheckImage,
                        cancellationToken);
                }
            }
            else
            {
                existingCheck.SetIssuer(new CustomerId(dto.CheckIssuerId!.Value));
                existingCheck.SetIssuerFinancialAccount(new FinancialAccountId(dto.CheckIssuerFinancialAccountId!.Value));
                existingCheck.SetNumber(dto.CheckNumber);
                existingCheck.SetSayadiCode(dto.CheckSayadiCode);
                existingCheck.SetDueDate(dto.CheckDueDate!.Value);

                await checkPaymentRepository.UpdateAsync(existingCheck, cancellationToken);

                if (dto.CheckImage != null && !string.IsNullOrEmpty(dto.CheckImageContentType))
                {
                    await fileService.ReplaceLocalFileAsync(
                        webHostEnvironment.GetCheckPaymentFilePath(payment.Id.Value, dto.CheckImageContentType), dto.CheckImage,
                        cancellationToken);
                }
            }
        }

        foreach (var payment in paymentsToDelete.Where(payment => payment.PaymentType == PaymentType.Check))
        {
            if (payment.CheckPayment is not null)
            {
                fileService.DeleteLocalFile(webHostEnvironment.GetCheckPaymentFilePath(payment.Id.Value));
            }
        }
    }

    private async Task UpdateTargetInvoicePaymentsAsync(InvoicePayment sourcePayment, CancellationToken cancellationToken = default)
    {
        // Guards
        if (sourcePayment.PaymentType != PaymentType.CustomerTransfer)
            return;

        if (sourcePayment.SourcePaymentId != null)
            return; // prevent recursion

        var existingTargetPayment = await paymentRepository
            .Get(new InvoicePaymentsBySourcePaymentIdSpecification(sourcePayment.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (!sourcePayment.TargetInvoiceId.HasValue)
        {
            if (existingTargetPayment != null) 
                repository.Delete(existingTargetPayment);

            return;
        }

        var targetInvoiceId = sourcePayment.TargetInvoiceId.Value;

        if (existingTargetPayment == null)
        {
            var targetPayment = InvoicePayment.Create(
                sourcePayment.PaymentDate,
                PaymentType.TransferedPayment,
                sourcePayment.PaymentSide == PaymentSide.Pay
                    ? PaymentSide.Receive
                    : PaymentSide.Pay,
                sourcePayment.Amount,
                sourcePayment.ExchangeRate,
                null,
                targetInvoiceId,
                sourcePayment.PriceUnitId,
                null,
                null,
                null,
                sourcePayment.Id,
                null,
                sourcePayment.Invoice?.InvoiceNumber.ToString(),
                PaymentNoteBuilder.BuildForTransfer(
                    sourcePayment.Invoice!.Customer!.FullName,
                    sourcePayment.Invoice.InvoiceNumber,
                    sourcePayment.Invoice.InvoiceType)
            );

            paymentRepository.Create(targetPayment);
        }
        else
        {
            existingTargetPayment.SetPaymentDate(sourcePayment.PaymentDate);
            existingTargetPayment.SetAmount(sourcePayment.Amount, sourcePayment.PriceUnitId);
            existingTargetPayment.SetExchangeRate(sourcePayment.ExchangeRate);
            existingTargetPayment.SetFinalAmount(sourcePayment.Amount, null);
            existingTargetPayment.SetInvoiceId(sourcePayment.TargetInvoiceId.Value);
            existingTargetPayment.SetReferenceNumber(
                sourcePayment.Invoice?.InvoiceNumber.ToString());
            existingTargetPayment.SetNote(
                PaymentNoteBuilder.BuildForTransfer(
                    sourcePayment.Invoice!.Customer!.FullName,
                    sourcePayment.Invoice.InvoiceNumber,
                    sourcePayment.Invoice.InvoiceType));

            paymentRepository.Update(existingTargetPayment);
        }
    }
}