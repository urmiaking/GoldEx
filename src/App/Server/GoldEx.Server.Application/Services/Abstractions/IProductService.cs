using GoldEx.Sdk.Common.Data;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IProductService
{
    Task CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<Product>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
}