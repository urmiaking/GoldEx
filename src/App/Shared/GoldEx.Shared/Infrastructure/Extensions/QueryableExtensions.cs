using System.Linq.Expressions;
using System.Reflection;
using GoldEx.Sdk.Common.Definitions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> SetTracking<TEntity>(this IQueryable<TEntity> source, bool tracking) where TEntity : class
    {
        return tracking ? source.AsTracking() : source.AsNoTracking();
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortLabel, SortDirection sortDirection,
        string defaultSortProperty = "LastModifiedDate") where T : class
    {
        var property = typeof(T).GetProperty(sortLabel, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        var parameter = Expression.Parameter(typeof(T), "x");

        if (property == null)
        {
            // Default sorting if property is invalid
            var defaultProperty = typeof(T).GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (defaultProperty == null) 
                return query; // If no default property, return the original query.

            var defaultPropertyAccess = Expression.MakeMemberAccess(parameter, defaultProperty);
            var defaultOrderByExpression = Expression.Lambda(defaultPropertyAccess, parameter);

            if (sortDirection == SortDirection.Ascending)
            {
                // Explicitly specify the type for default OrderBy
                var defaultOrderByMethod = typeof(Queryable)
                    .GetMethods()
                    .Single(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), defaultProperty.PropertyType);

                return (IQueryable<T>)defaultOrderByMethod.Invoke(null, [query, defaultOrderByExpression])!;
            }

            // Explicitly specify the type for default OrderByDescending
            var defaultOrderByDescendingMethod = typeof(Queryable)
                .GetMethods()
                .Single(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), defaultProperty.PropertyType);

            return (IQueryable<T>)defaultOrderByDescendingMethod.Invoke(null, [query, defaultOrderByExpression])!;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var orderByMethod = sortDirection == SortDirection.Ascending
            ? typeof(Queryable)
                .GetMethods()
                .Single(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType)
            : typeof(Queryable)
                .GetMethods()
                .Single(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, orderByExpression })!;
    }
}
