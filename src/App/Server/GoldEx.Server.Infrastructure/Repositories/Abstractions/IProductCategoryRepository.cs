using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IProductCategoryRepository : IRepository<ProductCategory>,
    ICreateRepository<ProductCategory>,
    IUpdateRepository<ProductCategory>,
    IDeleteRepository<ProductCategory>;