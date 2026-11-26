namespace GoldEx.Server.Common.Authorization;

/// <summary>Explicit opt-in for a reviewed, non-mutating demo API action.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DemoReadAttribute : Attribute;
