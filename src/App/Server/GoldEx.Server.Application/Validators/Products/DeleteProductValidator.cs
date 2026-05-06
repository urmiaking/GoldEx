using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class DeleteProductValidator: AbstractValidator<Product>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;

    public DeleteProductValidator(IInventoryStockRepository inventoryStockRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه کالا نمی تواند خالی باشد");
            //.MustAsync(NotInUse).WithMessage("این کالا در فاکتورهای ثبت شده استفاده شده است و قابل حذف نیست.");

        RuleFor(x => x.Id)
            .MustAsync(HasPositiveQuantityAsync)
            .WithMessage("این کالا قبلا فروخته شده است و امکان حذف ندارد");
    }

    private async Task<bool> HasPositiveQuantityAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var quantity = await _inventoryStockRepository.GetQuantityAsync(id, cancellationToken);
        return quantity >= 0;
    }

    //private async Task<bool> NotInUse(ProductId id, CancellationToken cancellationToken = default)
    //{
    //    return !await _invoiceRepository.ExistsAsync(new InvoicesByProductIdSpecification(id), cancellationToken);
    //}
}