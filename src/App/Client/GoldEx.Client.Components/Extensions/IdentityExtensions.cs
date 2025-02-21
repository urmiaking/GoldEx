using System.Security.Claims;

namespace GoldEx.Client.Components.Extensions;

public static class IdentityExtensions
{
    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        var fullNameClaim = principal.FindFirst(ClaimTypes.GivenName);

        if (fullNameClaim == null)
            return principal.Identity?.Name ?? "N/A";

        return $"{fullNameClaim.Value}".Trim();
    }
}