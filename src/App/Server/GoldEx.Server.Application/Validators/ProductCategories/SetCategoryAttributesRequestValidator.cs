using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Server.Application.Validators.ProductCategories;

[ScopedService]
internal class SetCategoryAttributesRequestValidator : AbstractValidator<SetCategoryAttributesRequest>
{
    public SetCategoryAttributesRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("شناسه دسته‌بندی الزامی است.");

        RuleForEach(x => x.Attributes).ChildRules(attr =>
        {
            attr.RuleFor(x => x.AttributeId)
                .NotEmpty().WithMessage("شناسه ویژگی الزامی است.");
        });
    }
}
