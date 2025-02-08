using System.Security.Claims;

namespace GoldEx.Client.Components.Extensions;

public static class IdentityExtensions
{
    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        var firstNameClaim = principal.FindFirst(ClaimTypes.GivenName);
        var lastNameClaim = principal.FindFirst(ClaimTypes.Surname);

        if (firstNameClaim == null && lastNameClaim == null)
            return principal.Identity?.Name ?? "N/A";

        return $"{firstNameClaim?.Value} {lastNameClaim?.Value}".Trim();
    }
}