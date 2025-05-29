using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Application.Validators.Products;

namespace GoldEx.Server.Application.Validators;

public class UpdateServerProductValidator : UpdateProductValidator<Product, ProductCategory, GemStone>
{
    public UpdateServerProductValidator(IProductRepository<Product, ProductCategory, GemStone> repository,
        IProductCategoryRepository<ProductCategory> categoryRepository) : base(repository,
        categoryRepository)
    {
        Include(new CreateServerProductValidator(repository, categoryRepository));
    }
}