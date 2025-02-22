using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IProductRepository : IRepository,
    ICreateRepository<Product>,
    IUpdateRepository<Product>,
    IDeleteRepository<Product>
{
    Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<Product>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
}