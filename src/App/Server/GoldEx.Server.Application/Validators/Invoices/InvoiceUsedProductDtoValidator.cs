using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceUsedProductDtoValidator : AbstractValidator<InvoiceUsedProductDto>
{
    public InvoiceUsedProductDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("لطفا عنوان/توضیحات را وارد کنید")
            .MaximumLength(100).WithMessage("عنوان/توضیحات نمی تواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000).WithMessage("عیار باید بین 0 و 1000 باشد.")
            .NotEmpty().WithMessage("لطفا عیار را وارد کنید");

        RuleFor(x => x.GramPrice)
            .GreaterThan(0).WithMessage("نرخ طلا باید بزرگتر از 0 باشد.")
            .NotEmpty().WithMessage("لطفا نرخ طلا را وارد کنید");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("وزن باید بزرگتر یا مساوی 0 باشد.")
            .NotEmpty().WithMessage("لطفا وزن را وارد کنید");

        RuleFor(x => x.ProductType)
            .Must(x => x is ProductType.Gold or ProductType.Jewelry)
            .WithMessage("نوع محصول باید طلا یا جواهر باشد.");
    }
}