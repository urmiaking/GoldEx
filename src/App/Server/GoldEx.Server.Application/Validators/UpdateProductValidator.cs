using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Application.Validators;

[ScopedService]
public class UpdateProductValidator : AbstractValidator<Product>
{
    public UpdateProductValidator()
    {
        Include(new CreateProductValidator());

        RuleFor(x => x.Id)
            .NotEqual(new ProductId(Guid.Empty)).WithMessage("شناسه کالا نمی تواند خالی باشد");
    }
}