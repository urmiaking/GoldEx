using Microsoft.EntityFrameworkCore;

namespace GoldEx.Sdk.Server.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> SetTracking<TEntity>(this IQueryable<TEntity> source, bool tracking) where TEntity : class
    {
        return tracking ? source.AsTracking() : source.AsNoTracking();
    }
}
