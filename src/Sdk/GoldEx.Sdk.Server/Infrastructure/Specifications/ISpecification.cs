using GoldEx.Sdk.Server.Domain.Entities;
using System.Linq.Expressions;

namespace GoldEx.Sdk.Server.Infrastructure.Specifications;

public interface ISpecification<TEntity> : ISpecification where TEntity : EntityBase
{
    Expression<Func<TEntity, bool>>? Criteria { get; }
    List<Expression<Func<TEntity, object>>> Includes { get; }
    Expression<Func<TEntity, object>>? OrderBy { get; }
    Expression<Func<TEntity, object>>? OrderByDescending { get; }
}

public interface ISpecification
{
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}