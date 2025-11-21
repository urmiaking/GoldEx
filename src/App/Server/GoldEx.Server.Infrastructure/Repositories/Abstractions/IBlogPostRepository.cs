using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BlogPostAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBlogPostRepository : IRepository<BlogPost>,
    ICreateRepository<BlogPost>,
    IUpdateRepository<BlogPost>,
    IDeleteRepository<BlogPost>;