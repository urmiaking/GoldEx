using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BlogCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBlogCategoryRepository : IRepository<BlogCategory>,
    ICreateRepository<BlogCategory>,
    IUpdateRepository<BlogCategory>,
    IDeleteRepository<BlogCategory>;