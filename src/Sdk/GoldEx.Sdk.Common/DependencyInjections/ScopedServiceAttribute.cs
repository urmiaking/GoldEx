namespace GoldEx.Sdk.Common.DependencyInjections;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScopedServiceAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScopedServiceAttribute<TService> : ScopedServiceAttribute;