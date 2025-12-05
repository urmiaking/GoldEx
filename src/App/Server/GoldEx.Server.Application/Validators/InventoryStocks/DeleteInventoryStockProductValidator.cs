using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.InventoryStocks;

[ScopedService]
public class DeleteInventoryStockProductValidator : AbstractValidator<Guid>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInventoryStockRepository _invoiceStockRepository;
    public DeleteInventoryStockProductValidator(IInvoiceRepository invoiceRepository, IInventoryStockRepository invoiceStockRepository)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceStockRepository = invoiceStockRepository;

        RuleFor(x => x)
            .MustAsync(NotEnteredByPurchasedInvoice)
            .WithMessage("این محصول از طریق فاکتور خرید اضافه شده است و امکان حذف آن وجود ندارد.")
            .MustAsync(NotResultInNegativeInventory)
            .WithMessage("حذف این محصول منجر به موجودی منفی در انبار می‌شود و امکان‌پذیر نیست.")
            .MustAsync(NotUsedInSellInvoices)
            .WithMessage("این محصول در فاکتورهای فروش استفاده شده است و امکان حذف آن وجود ندارد.");
    }

    private async Task<bool> NotEnteredByPurchasedInvoice(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _invoiceRepository
            .ExistsAsync(new InvoicesByProductIdSpecification(new ProductId(id), InvoiceType.Purchase), cancellationToken);

        return !exists;
    }

    private async Task<bool> NotResultInNegativeInventory(Guid id, CancellationToken cancellationToken)
    {
        var currentStock = await _invoiceStockRepository.GetQuantityAsync(new ProductId(id), cancellationToken);
        return currentStock > 0;
    }

    private async Task<bool> NotUsedInSellInvoices(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _invoiceRepository
            .ExistsAsync(new InvoicesByProductIdSpecification(new ProductId(id), InvoiceType.Sell), cancellationToken);

        return !exists;
    }
}