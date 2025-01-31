using Microsoft.AspNetCore.Authorization;

namespace GoldEx.Sdk.Common.Authorization;

public class PolicyDefinition
{
    private PolicyDefinition(string name, string description, AuthorizationPolicy policy, IEnumerable<PolicyDefinition> childPolicies)
    {
        Name = name;
        Description = description;
        Policy = policy;
        ChildPolicies = childPolicies;
    }

    public string Name { get; }
    public string Description { get; }
    public IEnumerable<PolicyDefinition> ChildPolicies { get; }

    public AuthorizationPolicy Policy { get; }

    public PolicyDefinition[] Definitions
    {
        get
        {
            PolicyDefinition[] me = [this];

            return [.. me, .. ChildPolicies.SelectMany(x => x.Definitions)];
        }
    }

    public AuthorizationPolicy[] Policies
    {
        get
        {
            AuthorizationPolicy[] myPolicy = [Policy];

            return [.. myPolicy, .. ChildPolicies.SelectMany(x => x.Policies)];
        }
    }

    public static PolicyDefinition Build(string name,
                                         string description,
                                         Action<AuthorizationPolicyBuilder> builder,
                                         IEnumerable<PolicyDefinition>? childPolicies = null)
    {
        var policyBuilder = new AuthorizationPolicyBuilder();
        builder.Invoke(policyBuilder);

        return new PolicyDefinition(name, description, policyBuilder.Build(), childPolicies ?? Enumerable.Empty<PolicyDefinition>());
    }

    public static PolicyDefinition Build(string claim,
                                         string description,
                                         IEnumerable<PolicyDefinition>? childPolicies = null)
    {
        return Build(claim,
                     description,
                     b => b.RequireClaim(claim),
                     childPolicies);
    }
}
