using System.Linq.Expressions;

namespace GoldEx.Sdk.Server.Infrastructure.Helpers;

internal class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
    : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node)
    {
        // Replace the old parameter with the new parameter
        return node == oldParameter ? newParameter : base.VisitParameter(node);
    }
}