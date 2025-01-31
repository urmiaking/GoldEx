
namespace GoldEx.Sdk.Common.Authorization;

public interface IApplicationPolicyProvider
{
    string Category { get; }
    IEnumerable<PolicyDefinition> GetPolicies();
}
