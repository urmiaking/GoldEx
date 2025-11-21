using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;

namespace GoldEx.Server.Application.Validators.Blogs;

[ScopedService]
internal sealed class BlogCategoryRequestValidator : AbstractValidator<BlogCategoryRequest>
{
    public BlogCategoryRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان دسته بندی الزامی است");
    }
}