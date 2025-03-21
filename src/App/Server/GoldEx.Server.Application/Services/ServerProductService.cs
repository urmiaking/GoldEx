using GoldEx.Server.Application.Validators;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Application.Services;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

public class ServerProductService(
    IProductRepository<Product, ProductCategory, GemStone> repository,
    CreateServerProductValidator createValidator,
    UpdateServerProductValidator updateValidator,
    DeleteProductValidator<Product, ProductCategory, GemStone> deleteValidator) : ProductService<Product, ProductCategory, GemStone>(repository,
    createValidator,
    updateValidator,
    deleteValidator);