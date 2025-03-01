using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IProductRepository<T> : IRepository,
    ICreateRepository<T>,
    IUpdateRepository<T>,
    IDeleteRepository<T> where T : EntityBase
{
    Task<T?> GetAsync(ProductId id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(string barcode, CancellationToken cancellationToken = default);
    Task<PagedList<T>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
}