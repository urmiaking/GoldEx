using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class DeleteInvoiceValidator : AbstractValidator<Invoice>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;
    private readonly IInvoicePaymentRepository _invoicePaymentRepository;

    public DeleteInvoiceValidator(IInventoryStockRepository inventoryStockRepository, 
        IInvoicePaymentRepository invoicePaymentRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;
        _invoicePaymentRepository = invoicePaymentRepository;

        When(x => x.InvoiceType == InvoiceType.Purchase, () =>
        {
            RuleFor(x => x)
                .MustAsync(NotResultInNegativeInventory)
                .WithMessage("حذف این فاکتور خرید باعث منفی شدن موجودی یک یا چند کالا در انبار می‌شود.");
        });

        RuleFor(x => x.Id)
            .MustAsync(NotUsedInCustomerTransferPayments)
            .WithMessage("این فاکتور در پرداخت حواله ای استفاده شده است. لطفا ابتدا پرداخت مربوط به حواله این فاکتور را حذف کرده سپس این فاکتور را حذف کنید");
    }

    private async Task<bool> NotUsedInCustomerTransferPayments(InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        var existingPayments = await _invoicePaymentRepository
            .Get(new InvoicePaymentsByInvoiceIdSpecification(invoiceId))
            .ToListAsync(cancellationToken);

        return !existingPayments.Any(x => x.SourcePaymentId.HasValue || x.TargetInvoiceId.HasValue);
    }

    private async Task<bool> NotResultInNegativeInventory(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var productIds = invoice.ProductItems.Select(p => p.ProductId).ToList();
        var coinIds = invoice.CoinItems.Select(c => c.CoinInstanceId).ToList();
        var currencyIds = invoice.CurrencyItems.Select(c => c.CurrencyId).ToList();

        var productQuantities = await _inventoryStockRepository.GetQuantitiesAsync(productIds, cancellationToken);
        var coinQuantities = await _inventoryStockRepository.GetQuantitiesAsync(coinIds, cancellationToken);
        var currencyQuantities = await _inventoryStockRepository.GetQuantitiesAsync(currencyIds, cancellationToken);

        foreach (var productItem in invoice.ProductItems)
        {
            var currentStock = productQuantities.GetValueOrDefault(productItem.ProductId, 0m);
            if (currentStock - 1 < 0)
            {
                return false;
            }
        }

        foreach (var coinItem in invoice.CoinItems)
        {
            var currentStock = coinQuantities.GetValueOrDefault(coinItem.CoinInstanceId, 0m);
            if (currentStock - coinItem.Quantity < 0)
            {
                return false;
            }
        }

        foreach (var currencyItem in invoice.CurrencyItems)
        {
            var currentStock = currencyQuantities.GetValueOrDefault(currencyItem.CurrencyId, 0m);
            if (currentStock - currencyItem.Amount < 0)
            {
                return false;
            }
        }

        return true;
    }
}