using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;

namespace GoldEx.Shared.Application.Validators.Products;

public class DeleteProductValidator<T> : AbstractValidator<T> where T : ProductBase
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