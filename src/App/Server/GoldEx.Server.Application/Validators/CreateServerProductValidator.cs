using FluentValidation;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Validators;

namespace GoldEx.Server.Application.Validators;

public class CreateServerProductValidator : CreateProductValidator<Product>
{
    public CreateServerProductValidator()
    {
        RuleFor(x => x.CreatedUserId)
            .NotEqual(Guid.Empty)
            .WithMessage("کاربر نامعتبر");
    }
}