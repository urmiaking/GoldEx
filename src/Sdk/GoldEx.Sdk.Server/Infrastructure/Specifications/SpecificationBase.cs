using GoldEx.Sdk.Server.Domain.Entities;
using System.Linq.Expressions;
using GoldEx.Sdk.Server.Infrastructure.Helpers;
using GoldEx.Sdk.Common.Definitions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace GoldEx.Sdk.Server.Infrastructure.Specifications;

public abstract class SpecificationBase<TEntity> : ISpecification<TEntity>
    where TEntity : EntityBase
{
    public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = [];
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationBase{TEntity}"/> class.
    /// </summary>
    /// <param name="initialCriteria">The initial criteria for the specification.</param>
    protected SpecificationBase(Expression<Func<TEntity, bool>>? initialCriteria = null)
    {
        Criteria = initialCriteria;
    }

    /// <summary>
    /// Adds an include expression to the specification.
    /// </summary>
    /// <param name="includeExpression">The expression representing the navigation property to include.</param>
    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds another criteria to the specification.
    /// If existing criteria are present, the new criteria will be combined using an AND operator.
    /// </summary>
    /// <param name="newCriteria">The new criteria to add.</param>
    protected void AddCriteria(Expression<Func<TEntity, bool>> newCriteria)
    {
        if (newCriteria == null) throw new ArgumentNullException(nameof(newCriteria));

        if (Criteria == null)
        {
            Criteria = newCriteria;
        }
        else
        {
            // Combine the existing criteria with the new one using 'AND'
            // We need to rewrite the expression to use the same parameter
            // for both expressions.

            var originalParameter = Criteria.Parameters[0];
            var newParameter = newCriteria.Parameters[0];

            // Create a visitor to replace the parameter in the newCriteria
            // with the parameter from the original Criteria.
            var visitor = new ParameterReplacer(newParameter, originalParameter);
            var rewrittenNewCriteriaBody = visitor.Visit(newCriteria.Body);

            if (rewrittenNewCriteriaBody == null)
            {
                // This should ideally not happen if newCriteria.Body is valid.
                // Handle error or throw a more specific exception.
                throw new InvalidOperationException("Failed to rewrite new criteria body.");
            }

            var andAlsoExpression = Expression.AndAlso(Criteria.Body, rewrittenNewCriteriaBody);
            Criteria = Expression.Lambda<Func<TEntity, bool>>(andAlsoExpression, originalParameter);
        }
    }


    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Applies ordering by a specific property in ascending order.
    /// </summary>
    /// <param name="orderByExpression">The expression representing the property to order by.</param>
    protected void ApplyOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        OrderByDescending = null; // Clear descending order if ascending is set
    }

    /// <summary>
    /// Applies ordering by a specific property in descending order.
    /// </summary>
    /// <param name="orderByDescendingExpression">The expression representing the property to order by descending.</param>
    protected void ApplyOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null; // Clear ascending order if descending is set
    }

    /// <summary>
    /// Applies sorting to the specification based on a string property label.
    /// Supports nested properties (e.g., "Asset.Name").
    /// </summary>
    /// <param name="sortLabel">The string representation of the property to sort by (e.g., "Name", "Asset.TypeId.Value").</param>
    /// <param name="sortDirection">The direction of the sort (Ascending or Descending).</param>
    /// <param name="defaultSortProperty">The property to sort by if sortLabel is null, empty, or invalid. Defaults to "Id".</param>
    protected void ApplySorting(string? sortLabel, SortDirection sortDirection, string defaultSortProperty = "CreatedAt")
    {
        if (string.IsNullOrWhiteSpace(sortLabel))
        {
            ApplyDefaultSortInternal(sortDirection, defaultSortProperty);
            return;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression propertyExpression = parameter;
        var currentPropertyType = typeof(TEntity); // Keep track of the current property's type for nested access

        try
        {
            var propertyPaths = sortLabel.Split('.');
            foreach (var propertyName in propertyPaths)
            {
                var propertyInfo = currentPropertyType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    // Property not found in the current type. Fallback to default sorting.
                    Console.WriteLine($"Warning: Property '{string.Join(".", propertyPaths)}' not found for sorting. Falling back to default '{defaultSortProperty}'.");
                    ApplyDefaultSortInternal(sortDirection, defaultSortProperty);
                    return;
                }
                propertyExpression = Expression.MakeMemberAccess(propertyExpression, propertyInfo);
                currentPropertyType = propertyInfo.PropertyType; // Update for next level of nesting
            }

            // The OrderBy/OrderByDescending expressions expect Expression<Func<TEntity, object>>.
            // If propertyType is a value type (e.g. int, DateTime), it needs to be converted to object.
            var convertedPropertyExpression = Expression.Convert(propertyExpression, typeof(object));
            var orderByLambda = Expression.Lambda<Func<TEntity, object>>(convertedPropertyExpression, parameter);

            if (sortDirection == SortDirection.Ascending)
            {
                ApplyOrderBy(orderByLambda);
            }
            else
            {
                ApplyOrderByDescending(orderByLambda);
            }
        }
        catch (Exception ex) // Consider more specific exception handling
        {
            Console.WriteLine($"Error applying sorting for '{sortLabel}': {ex.Message}. Falling back to default '{defaultSortProperty}'.");
            ApplyDefaultSortInternal(sortDirection, defaultSortProperty);
        }
    }

    /// <summary>
    /// Applies the default sorting logic.
    /// </summary>
    private void ApplyDefaultSortInternal(SortDirection sortDirection, string defaultSortProperty)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var propertyInfo = typeof(TEntity).GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
        {
            // If default property is not found, do nothing or log a warning.
            // Cannot sort if the property doesn't exist.
            Console.WriteLine($"Warning: Default sort property '{defaultSortProperty}' not found on type '{typeof(TEntity).Name}'. No sorting applied.");
            return;
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
        var convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(object)); // Convert to object
        var defaultOrderByLambda = Expression.Lambda<Func<TEntity, object>>(convertedPropertyAccess, parameter);

        if (sortDirection == SortDirection.Ascending)
        {
            ApplyOrderBy(defaultOrderByLambda);
        }
        else
        {
            ApplyOrderByDescending(defaultOrderByLambda);
        }
    }
}
