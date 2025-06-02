using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class DeleteProductValidator: AbstractValidator<Product>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteProductValidator(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه کالا نمی تواند خالی باشد")
            .MustAsync(NotInUse).WithMessage("این کالا در فاکتورهای ثبت شده استفاده شده است و قابل حذف نیست.");
    }

    private async Task<bool> NotInUse(ProductId id, CancellationToken cancellationToken = default)
    {
        return !await _invoiceRepository.ExistsAsync(new InvoicesByProductIdSpecification(id), cancellationToken);
    }
}