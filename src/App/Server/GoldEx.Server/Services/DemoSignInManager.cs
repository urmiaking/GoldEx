using System.Security.Claims;
using GoldEx.Sdk.Server.Api.Identity;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Services;

/// <summary>Central Identity check for password, passkey and all other sign-in paths.</summary>
public sealed class DemoSignInManager(
    UserManager<AppUser> userManager,
    IHttpContextAccessor accessor,
    IUserClaimsPrincipalFactory<AppUser> factory,
    IOptions<IdentityOptions> options,
    ILogger<SignInManager<AppUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<AppUser> confirmation,
    IEnumerable<IClaimProvider<AppUser>> providers,
    DemoAccessService demo)
    : GoldExSignInManager<AppUser>(userManager, accessor, factory, options, logger, schemes, confirmation, providers)
{
    public override async Task<bool> CanSignInAsync(AppUser user)
    {
        if (!await base.CanSignInAsync(user))
            return false;
        var status = await demo.GetStatusAsync(user, Context.Request.Host.Host, Context.RequestAborted);
        return !status.IsRestricted || status.Session is not null;
    }

    public override async Task SignInWithClaimsAsync(AppUser user, AuthenticationProperties? properties,
        IEnumerable<Claim> additionalClaims)
    {
        var status = await demo.GetStatusAsync(user, Context.Request.Host.Host, Context.RequestAborted);
        if (status.IsRestricted)
        {
            if (status.Session is not { } session)
                throw new InvalidOperationException("Demo account is not available.");
            properties ??= new AuthenticationProperties();
            if (properties.ExpiresUtc is null || properties.ExpiresUtc > session.ExpiresAt)
                properties.ExpiresUtc = session.ExpiresAt;
            properties.AllowRefresh = false;
        }
        await base.SignInWithClaimsAsync(user, properties, additionalClaims);
    }
}
