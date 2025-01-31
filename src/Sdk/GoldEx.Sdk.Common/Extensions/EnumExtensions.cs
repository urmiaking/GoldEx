using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GoldEx.Sdk.Common.Extensions;

public static class EnumExtensions
{
    internal static TAttribute? GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
    {
        var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
        return memberInfo == null ? null : memberInfo.GetCustomAttribute<TAttribute>();
    }

    public static string GetDisplayName(this Enum enumValue)
    {
        var attribute = enumValue.GetAttribute<DisplayAttribute>();
        return !string.IsNullOrEmpty(attribute?.Name) ? attribute.Name : enumValue.ToString();
    }
}
