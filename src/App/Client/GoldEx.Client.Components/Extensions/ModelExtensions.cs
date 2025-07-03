using System.Linq.Expressions;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Components.Extensions;

public static class ModelExtensions
{
    public static string GetDisplayName<TProperty>(this object model, Expression<Func<TProperty>> expression)
    {
        return expression.GetDisplayName();
    }
}
