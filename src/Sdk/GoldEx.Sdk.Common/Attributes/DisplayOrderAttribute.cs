namespace GoldEx.Sdk.Common.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DisplayOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
