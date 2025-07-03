using System.Linq.Expressions;
using System.Reflection;

namespace GoldEx.Sdk.Common.Extensions;

public static class ExpressionExtensions
{
    public static IReadOnlyList<MemberInfo> GetPropertyAccessList(this LambdaExpression expression)
    {
        var propertyVisitor = new PropertyVisitor();
        propertyVisitor.Visit(expression.Body);
        propertyVisitor.Path.Reverse();
        return propertyVisitor.Path;
    }

    public static string? GetMemberName<T>(this Expression<T> expression)
    {
        return expression.GetMemberInfo()?.Name;
    }

    public static MemberInfo? GetMemberInfo<T>(this Expression<T> expression)
    {
        MemberInfo? memberInfo;
        switch (expression.Body)
        {
            case MemberExpression memberExpression:
                memberInfo = memberExpression.Member;
                break;
            case UnaryExpression unaryExpression:
                if (unaryExpression.Operand is MemberExpression operand)
                {
                    memberInfo = operand.Member;
                    break;
                }
                goto default;
            default:
                memberInfo = null;
                break;
        }
        return memberInfo;
    }

    public static string GetDisplayName<T>(this Expression<T> expression)
    {
        var memberInfo = expression.GetMemberInfo();
        return memberInfo.GetDisplayName();
    }

    public static string? GetPrompt<T>(this Expression<T> expression)
    {
        var memberInfo = expression.GetMemberInfo();
        return memberInfo.GetPrompt();
    }

    private class PropertyVisitor : ExpressionVisitor
    {
        internal List<MemberInfo> Path { get; } = [];

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member as PropertyInfo == null)
                throw new ArgumentException("The path can only contain properties", nameof(node));
            Path.Add(node.Member);
            return base.VisitMember(node);
        }
    }
}