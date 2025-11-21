using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BlogCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class BlogCategoryRepository(GoldExDbContext dbContext) : RepositoryBase<BlogCategory>(dbContext), IBlogCategoryRepository;