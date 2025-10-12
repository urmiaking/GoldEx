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

    public static IQueryable<T> ApplySorting<T>(
    this IQueryable<T> query,
    string? sortLabel,
    SortDirection sortDirection,
    string defaultSortProperty = "CreatedAt") where T : class
    {
        if (string.IsNullOrEmpty(sortLabel))
            return ApplyDefaultSort(query, sortDirection, defaultSortProperty);

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
                    return ApplyDefaultSort(query, sortDirection, defaultSortProperty);

                var memberAccess = Expression.MakeMemberAccess(propertyExpression, property);

                if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyExpression = Expression.Condition(
                        Expression.Equal(propertyExpression, Expression.Constant(null, propertyExpression.Type)),
                        Expression.Constant(GetDefaultValue(property.PropertyType), property.PropertyType),
                        memberAccess);
                }
                else
                {
                    propertyExpression = memberAccess;
                }

                propertyType = property.PropertyType;
            }

            var lambda = Expression.Lambda(propertyExpression, parameter);
            var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var orderByMethod = typeof(Queryable).GetMethods()
                .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
        }
        catch
        {
            return ApplyDefaultSort(query, sortDirection, defaultSortProperty);
        }
    }

    private static object? GetDefaultValue(Type type)
    {
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }

    // This is the method with the key change.
    private static IQueryable<T> ApplyDefaultSort<T>(this IQueryable<T> query, SortDirection sortDirection, string defaultSortProperty) where T : class
    {
        var propertyPaths = defaultSortProperty.Split('.');
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
                    // If even the default property path is invalid, we can't sort.
                    return query;
                }
                propertyExpression = Expression.MakeMemberAccess(propertyExpression, property);
                propertyType = property.PropertyType;
            }

            var defaultOrderByExpression = Expression.Lambda(propertyExpression, parameter);
            var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var defaultOrderByMethod = typeof(Queryable).GetMethods()
                .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IQueryable<T>)defaultOrderByMethod.Invoke(null, new object[] { query, defaultOrderByExpression })!;
        }
        catch (Exception)
        {
            // If building the default sort expression fails for any reason, return the original query.
            return query;
        }
    }
}
