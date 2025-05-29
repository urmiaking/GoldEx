using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IProductRepository : IRepository<Product>,
    ICreateRepository<Product>,
    IUpdateRepository<Product>,
    IDeleteRepository<Product>;