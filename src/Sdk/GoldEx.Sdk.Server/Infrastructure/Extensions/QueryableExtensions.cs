using System.Linq.Expressions;
using System.Reflection;
using GoldEx.Sdk.Common.Definitions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Sdk.Server.Infrastructure.Extensions;

public static class QueryableExtensions
{
    extension<TEntity>(IQueryable<TEntity> source) where TEntity : class
    {
        public IQueryable<TEntity> SetTracking(bool tracking)
        {
            return tracking ? source.AsTracking() : source.AsNoTracking();
        }

        public IQueryable<TEntity> ApplySorting(string? sortLabel,
            SortDirection sortDirection,
            string defaultSortProperty = "CreatedAt")
        {
            if (string.IsNullOrEmpty(sortLabel))
                return ApplyDefaultSort(source, sortDirection, defaultSortProperty);

            var propertyPaths = sortLabel.Split('.');
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression propertyExpression = parameter;
            var propertyType = typeof(TEntity);

            try
            {
                foreach (var propertyName in propertyPaths)
                {
                    var property = propertyType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                        return ApplyDefaultSort(source, sortDirection, defaultSortProperty);

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
                    .MakeGenericMethod(typeof(TEntity), propertyType);

                return (IQueryable<TEntity>)orderByMethod.Invoke(null, [source, lambda])!;
            }
            catch
            {
                return ApplyDefaultSort(source, sortDirection, defaultSortProperty);
            }
        }
    }

    private static object? GetDefaultValue(Type type)
    {
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }

    extension<T>(IQueryable<T> query) where T : class
    {
        private IQueryable<T> ApplyDefaultSort(SortDirection sortDirection, string defaultSortProperty)
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

                return (IQueryable<T>)defaultOrderByMethod.Invoke(null, [query, defaultOrderByExpression])!;
            }
            catch (Exception)
            {
                // If building the default sort expression fails for any reason, return the original query.
                return query;
            }
        }
    }
}
