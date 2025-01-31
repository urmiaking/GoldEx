using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Sdk.Common.Authorization;

public class AuthorizationPolicyProvider(IServiceProvider serviceProvider) : IAuthorizationPolicyProvider
{
    private readonly IEnumerable<IApplicationPolicyProvider> _providers = serviceProvider.GetServices<IApplicationPolicyProvider>();

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policyDef = _providers
            .SelectMany(x => x.GetPolicies().SelectMany(d => d.Definitions))
            .FirstOrDefault(x => x.Name == policyName);

        if (policyDef == null)
            return GetDefaultPolicyAsync()!;

        return Task.FromResult(policyDef.Policy)!;
    }
}
