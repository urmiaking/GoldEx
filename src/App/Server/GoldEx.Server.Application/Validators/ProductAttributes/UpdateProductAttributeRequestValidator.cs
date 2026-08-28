using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.ProductAttributes;

namespace GoldEx.Server.Application.Validators.ProductAttributes;

[ScopedService]
internal class UpdateProductAttributeRequestValidator : AbstractValidator<UpdateProductAttributeRequest>
{
    public UpdateProductAttributeRequestValidator(IProductAttributeRepository repository)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان ویژگی نمی‌تواند خالی باشد.")
            .MaximumLength(100).WithMessage("حداکثر طول عنوان ویژگی ۱۰۰ کاراکتر می‌باشد.");

        RuleFor(x => x.Unit)
            .MaximumLength(30).WithMessage("حداکثر طول واحد اندازه‌گیری ۳۰ کاراکتر می‌باشد.")
            .When(x => !string.IsNullOrEmpty(x.Unit));

        RuleFor(x => x.Options)
            .MaximumLength(1000).WithMessage("حداکثر طول گزینه‌ها ۱۰۰۰ کاراکتر می‌باشد.")
            .When(x => !string.IsNullOrEmpty(x.Options));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("حداکثر طول توضیحات ۵۰۰ کاراکتر می‌باشد.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
