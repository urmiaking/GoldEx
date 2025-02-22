using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Application.Validators;

[ScopedService]
public class DeleteProductValidator : AbstractValidator<Product>
{
    // TODO: add invoice repository and check the product is in use in any invoice
    public DeleteProductValidator()
    {
        //RuleFor(x => x)
        //    .MustAsync(async (product, cancellationToken) =>
        //    {

        //    });
    }
}