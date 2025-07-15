using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceRequestDtoValidator : AbstractValidator<InvoiceRequestDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly IProductRepository _productRepository;
    public InvoiceRequestDtoValidator(CustomerRequestDtoValidator customerValidator,
        IPriceUnitRepository priceUnitRepository, IProductRepository productRepository, IProductCategoryRepository productCategoryRepository,
        IPaymentMethodRepository paymentMethodRepository, IInvoiceRepository invoiceRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _productRepository = productRepository;
        _invoiceRepository = invoiceRepository;

        RuleFor(x => x.InvoiceNumber)
            .GreaterThan(0)
            .WithMessage("لطفا شماره فاکتور را وارد کنید")
            .MustAsync(BeUniqueNumber)
            .WithMessage("شماره فاکتور نمی تواند تکراری باشد");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("تاریخ فاکتور الزامی است");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.InvoiceDate)
            .When(x => x.DueDate.HasValue)
            .WithMessage("تاریخ سررسید نمی‌تواند قبل از تاریخ فاکتور باشد");

        RuleFor(x => x.Customer)
            .NotNull().WithMessage("اطلاعات مشتری الزامی است")
            .SetValidator(customerValidator);

        RuleFor(x => x.InvoiceItems)
            
            .NotEmpty().WithMessage("فاکتور باید حداقل دارای یک آیتم باشد");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی فاکتور معتبر نمی باشد");

        RuleForEach(x => x.InvoiceItems)
            .SetValidator(new InvoiceItemDtoValidator(
                productCategoryRepository,
                productRepository,
                priceUnitRepository))
            .MustAsync(ProductAvailable)
            .WithMessage("این جنس قبلا فروخته شده است");

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentDtoValidator(priceUnitRepository, paymentMethodRepository));
    }

    private async Task<bool> ProductAvailable(InvoiceRequestDto invoiceDto, InvoiceItemDto invoiceItem, CancellationToken cancellationToken = default)
    {
        if (!invoiceItem.Product.Id.HasValue)
            return true;

        var productId = new ProductId(invoiceItem.Product.Id.Value);

        var product = await _productRepository
            .Get(new ProductsByIdSpecification(productId))
            .FirstOrDefaultAsync(cancellationToken);

        if (product?.InvoiceItem is null)
            return true;

        if (invoiceDto.Id.HasValue)
        {
            var existingInvoice = await _invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(invoiceDto.Id.Value)))
                .Include(x => x.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingInvoice != null && existingInvoice.Items.Any(item => item.ProductId == productId))
                return true;
        }

        return false;
    }

    private async Task<bool> BeValidPriceUnit(Guid id, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(id)), cancellationToken);
    }

    private async Task<bool> BeUniqueNumber(InvoiceRequestDto request, long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var item = await _invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.Id;
    }
}