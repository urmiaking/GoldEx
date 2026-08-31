using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class ProductAttributeRepository(GoldExDbContext dbContext) : RepositoryBase<ProductAttribute>(dbContext), IProductAttributeRepository
{
}
