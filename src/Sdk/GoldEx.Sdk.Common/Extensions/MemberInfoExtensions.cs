using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;

namespace GoldEx.Sdk.Common.Extensions;

public static class MemberInfoExtensions
{
    public static string GetDisplayName(this MemberInfo? member)
    {
        var customAttribute1 = member.GetCustomAttribute<DisplayAttribute>();

        if (customAttribute1 != null && !string.IsNullOrEmpty(customAttribute1.Name))
            return customAttribute1.Name;

        var customAttribute2 = member.GetCustomAttribute<DisplayNameAttribute>();
        return customAttribute2 != null && !string.IsNullOrEmpty(customAttribute2.DisplayName) ? customAttribute2.DisplayName : member.Name;
    }

    public static string? GetPrompt(this MemberInfo? member)
    {
        var customAttribute = member.GetCustomAttribute<DisplayAttribute>();
        return customAttribute != null && !string.IsNullOrEmpty(customAttribute.Prompt) ? customAttribute.Prompt : null;
    }
}