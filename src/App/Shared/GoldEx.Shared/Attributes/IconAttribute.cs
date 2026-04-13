namespace GoldEx.Shared.Attributes;

// Custom attribute
[AttributeUsage(AttributeTargets.Field)]
public class IconAttribute(string title) : Attribute
{
    public string Title { get; } = title;
}