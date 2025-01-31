using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Sdk.Common.DependencyInjections.Extensions;

public static class DependencyExtensions
{
    /// <summary>
    /// Discovers services decorated by <see cref="SingletonServiceAttribute"/> or <see cref="ScopedServiceAttribute"/> from the given assembly.
    /// </summary>
    /// <param name="services">Instance of the service collection</param>
    /// <param name="assembly">The target assembly to be discovered</param>
    /// <returns>Returns the service collection</returns>
    public static IServiceCollection DiscoverServices(this IServiceCollection services, Assembly assembly)
    {
        services.DiscoverSingletonServices(assembly);
        services.DiscoverScopedServices(assembly);
        services.DiscoverTransientServices(assembly);

        return services;
    }

    /// <summary>
    /// Discovers services decorated by <see cref="SingletonServiceAttribute"/> or <see cref="ScopedServiceAttribute"/> from calling assembly.
    /// </summary>
    /// <param name="services">Instance of the service collection</param>
    /// <returns>Returns the service collection</returns>
    public static IServiceCollection DiscoverServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly();

        services.DiscoverServices(assembly);

        return services;
    }

    private static void DiscoverSingletonServices(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var serviceAttrs = type.GetCustomAttributes(false)
                .Where(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(SingletonServiceAttribute<>) ||
                            x.GetType() == typeof(SingletonServiceAttribute));

            foreach (var serviceAttr in serviceAttrs)
            {
                var attributeType = serviceAttr.GetType();
                var implementationType = type;
                var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                var allInterfaces = implementationType.GetInterfaces();
                var directInterfaces = allInterfaces
                    .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                    .Except(baseTypeInterfaces);

                if (attributeType.IsGenericType)
                {
                    var serviceType = attributeType.GenericTypeArguments[0];

                    services.AddSingleton(serviceType, implementationType);
                }
                else if (implementationType.GetInterfaces().Any())
                {
                    foreach (var interfaceType in directInterfaces)
                    {
                        services.AddSingleton(interfaceType, implementationType);
                    }
                }
                else
                {
                    services.AddSingleton(implementationType);
                }
            }
        }
    }

    private static void DiscoverScopedServices(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var serviceAttrs = type.GetCustomAttributes(false)
                .Where(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(ScopedServiceAttribute<>) ||
                            x.GetType() == typeof(ScopedServiceAttribute));

            foreach (var serviceAttr in serviceAttrs)
            {
                var attributeType = serviceAttr.GetType();
                var implementationType = type;
                var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                var allInterfaces = implementationType.GetInterfaces();
                var directInterfaces = allInterfaces
                    .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                    .Except(baseTypeInterfaces);

                if (attributeType.IsGenericType)
                {
                    var serviceType = attributeType.GenericTypeArguments[0];

                    services.AddScoped(serviceType, implementationType);
                }
                else if (directInterfaces.Any())
                {
                    foreach (var interfaceType in directInterfaces)
                    {
                        services.AddScoped(interfaceType, implementationType);
                    }
                }
                else
                {
                    services.AddScoped(implementationType);
                }
            }
        }
    }

    private static void DiscoverTransientServices(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var serviceAttrs = type.GetCustomAttributes(false)
                .Where(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(TransientServiceAttribute<>) ||
                            x.GetType() == typeof(TransientServiceAttribute));

            foreach (var serviceAttr in serviceAttrs)
            {
                var attributeType = serviceAttr.GetType();
                var implementationType = type;
                var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                var allInterfaces = implementationType.GetInterfaces();
                var directInterfaces = allInterfaces
                    .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                    .Except(baseTypeInterfaces);

                if (attributeType.IsGenericType)
                {
                    var serviceType = attributeType.GenericTypeArguments[0];

                    services.AddTransient(serviceType, implementationType);
                }
                else if (directInterfaces.Any())
                {
                    foreach (var interfaceType in directInterfaces)
                    {
                        services.AddTransient(interfaceType, implementationType);
                    }
                }
                else
                {
                    services.AddTransient(implementationType);
                }
            }
        }
    }
}
