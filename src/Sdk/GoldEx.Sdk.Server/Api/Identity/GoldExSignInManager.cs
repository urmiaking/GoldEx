using System.Security.Claims;
using GoldEx.Sdk.Server.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldEx.Sdk.Server.Api.Identity;

public class GoldExSignInManager<TUser>(
    UserManager<TUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<TUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<TUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<TUser> confirmation,
    IEnumerable<IClaimProvider<TUser>> claimProviders)
    : SignInManager<TUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    where TUser : class
{
    private const string IdentityPrefix = "Identity";
    public static readonly string GoldExScheme = IdentityPrefix + ".GoldExToken";

    public override async Task SignInWithClaimsAsync(TUser user, AuthenticationProperties? authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        var claims = new List<Claim>();

        foreach (var provider in claimProviders)
        {
            var list = await provider.GetClaimsAsync(user);

            claims.AddRange(list);
        }

        await base.SignInWithClaimsAsync(user, authenticationProperties, [.. claims, .. additionalClaims]);
    }

    public async Task PasswordTokenSignIn(Guid userId)
    {
        await Context.SignInAsync(GoldExScheme, StoreTokenInfo(new GoldExIdentityInfo(userId, "GoldEx")));
    }

    public async Task<GoldExIdentityInfo?> GetStoredTokenInfo()
    {
        var result = await Context.AuthenticateAsync(GoldExScheme);

        var userIdStr = result.Principal?.FindFirstValue(ClaimTypes.Name);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
        {
            return null;
        }

        var method = result.Principal?.FindFirstValue(ClaimTypes.AuthenticationMethod);

        return new GoldExIdentityInfo(userId, method);
    }

    private ClaimsPrincipal StoreTokenInfo(GoldExIdentityInfo info)
    {
        var identity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, info.UserId.ToString()));
        if (info.LoginProvider != null)
        {
            identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, info.LoginProvider));
        }
        return new ClaimsPrincipal(identity);
    }
}
