﻿using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

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