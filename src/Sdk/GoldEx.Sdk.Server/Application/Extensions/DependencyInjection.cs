using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Sdk.Server.Application.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the user context accessor service, enabling access to the current user's context via dependency injection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the user context accessor to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}