using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductAttributes;
using GoldEx.Shared.DTOs.ProductAttributes;

namespace GoldEx.Server.Application.Validators.ProductAttributes;

[ScopedService]
internal class CreateProductAttributeRequestValidator : AbstractValidator<CreateProductAttributeRequest>
{
    private readonly IProductAttributeRepository _repository;

    public CreateProductAttributeRequestValidator(IProductAttributeRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان ویژگی نمی‌تواند خالی باشد.")
            .MaximumLength(100).WithMessage("حداکثر طول عنوان ویژگی ۱۰۰ کاراکتر می‌باشد.")
            .MustAsync(BeUniqueTitle).WithMessage("عنوان ویژگی نمی‌تواند تکراری باشد.");

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

    private async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title)) return true;
        return !await _repository.ExistsAsync(new ProductAttributesByTitleSpecification(title), cancellationToken);
    }
}
