using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAttributeAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IProductAttributeRepository : IRepository<ProductAttribute>,
    ICreateRepository<ProductAttribute>,
    IUpdateRepository<ProductAttribute>,
    IDeleteRepository<ProductAttribute>
{
}
