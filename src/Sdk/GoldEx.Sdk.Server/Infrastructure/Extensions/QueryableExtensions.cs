using System.Linq.Expressions;
using System.Reflection;
using GoldEx.Sdk.Common.Definitions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Sdk.Server.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> SetTracking<TEntity>(this IQueryable<TEntity> source, bool tracking) where TEntity : class
    {
        return tracking ? source.AsTracking() : source.AsNoTracking();
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortLabel, SortDirection sortDirection,
        string defaultSortProperty = "CreatedAt") where T : class
    {
        if (string.IsNullOrEmpty(sortLabel))
        {
            // Handle empty sort label gracefully, e.g., return query as is or sort by default.
            return ApplyDefaultSort(query, sortDirection, defaultSortProperty);
        }

        var propertyPaths = sortLabel.Split('.');
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression propertyExpression = parameter;
        var propertyType = typeof(T);

        try
        {
            foreach (var propertyName in propertyPaths)
            {
                var property = propertyType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    // Property not found in the current type. Fallback to default sorting.
                    return ApplyDefaultSort(query, sortDirection, defaultSortProperty);
                }

                propertyExpression = Expression.MakeMemberAccess(propertyExpression, property);
                propertyType = property.PropertyType;
            }


            var orderByExpression = Expression.Lambda(propertyExpression, parameter);

            var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";

            var orderByMethod = typeof(Queryable).GetMethods()
                .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IQueryable<T>)orderByMethod.Invoke(null, [query, orderByExpression])!;

        }
        catch (Exception)
        {
            //Exception while building expression (e.g., type mismatch). Fallback to default sorting
            return ApplyDefaultSort(query, sortDirection, defaultSortProperty);
        }
    }

    private static IQueryable<T> ApplyDefaultSort<T>(this IQueryable<T> query, SortDirection sortDirection, string defaultSortProperty) where T : class
    {
        var defaultProperty = typeof(T).GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (defaultProperty == null)
        {
            return query; // If no default property, return the original query.
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var defaultPropertyAccess = Expression.MakeMemberAccess(parameter, defaultProperty);
        var defaultOrderByExpression = Expression.Lambda(defaultPropertyAccess, parameter);

        var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";

        var defaultOrderByMethod = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), defaultProperty.PropertyType);

        return (IQueryable<T>)defaultOrderByMethod.Invoke(null, [query, defaultOrderByExpression])!;
    }
}
