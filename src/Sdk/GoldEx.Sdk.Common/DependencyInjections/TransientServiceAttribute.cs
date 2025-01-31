namespace GoldEx.Sdk.Common.DependencyInjections;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientServiceAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientServiceAttribute<TService> : TransientServiceAttribute;