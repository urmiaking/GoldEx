using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceRequestDtoValidator : AbstractValidator<InvoiceRequestDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceRequestDtoValidator(CustomerRequestDtoValidator customerValidator,
        IPriceUnitRepository priceUnitRepository, IProductRepository productRepository, IProductCategoryRepository productCategoryRepository,
        IPaymentMethodRepository paymentMethodRepository, IInvoiceRepository invoiceRepository)
    {
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
            .NotEmpty()
            .NotEmpty().WithMessage("فاکتور باید حداقل دارای یک آیتم باشد");

        RuleForEach(x => x.InvoiceItems)
            .SetValidator(new InvoiceItemDtoValidator(productCategoryRepository, productRepository, priceUnitRepository));

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentDtoValidator(priceUnitRepository, paymentMethodRepository));
    }

    private async Task<bool> BeUniqueNumber(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        return !await _invoiceRepository.ExistsAsync(new InvoicesByNumberSpecification(invoiceNumber), cancellationToken);
    }
}