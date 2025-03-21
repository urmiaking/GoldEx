using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;

namespace GoldEx.Shared.Application.Validators.Products;

public class DeleteProductValidator<TProduct, TCategory, TGemStone> : AbstractValidator<TProduct>
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
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