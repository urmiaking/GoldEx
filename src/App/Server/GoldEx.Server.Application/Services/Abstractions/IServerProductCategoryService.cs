using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerProductCategoryService
{
    Task<ProductCategory> GetOrCreateAsync(string? categoryTitle, CancellationToken cancellationToken = default);
}