using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;

namespace GoldEx.Server.Application.Validators.Blogs;

[ScopedService]
internal sealed class BlogPostRequestValidator : AbstractValidator<BlogPostRequest>
{
    public BlogPostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان پست الزامی است");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("نامک پست الزامی است");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("محتوا الزامی است.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("دسته بندی پست الزامی است.");
    }
}