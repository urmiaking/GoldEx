using GoldEx.Server.Application.Validators;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Services;
using GoldEx.Shared.Application.Validators;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

public class ServerProductService(
    IProductRepository<Product> repository,
    CreateServerProductValidator createValidator,
    UpdateServerProductValidator updateValidator,
    DeleteProductValidator<Product> deleteValidator) : ProductService<Product>(repository,
    createValidator,
    updateValidator,
    deleteValidator)
{
    
}