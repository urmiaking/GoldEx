using System.Security.Claims;

namespace GoldEx.Sdk.Server.Application.Abstractions;

public interface IClaimProvider<in TUser>
{
    Task<List<Claim>> GetClaimsAsync(TUser user);
}
