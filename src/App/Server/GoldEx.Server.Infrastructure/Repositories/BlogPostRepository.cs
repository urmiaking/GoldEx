using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BlogPostAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class BlogPostRepository(GoldExDbContext dbContext) : RepositoryBase<BlogPost>(dbContext), IBlogPostRepository;