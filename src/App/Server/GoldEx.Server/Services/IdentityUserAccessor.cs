using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Identity;

namespace GoldEx.Server.Services;

[ScopedService]
internal sealed class IdentityUserAccessor(UserManager<AppUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<AppUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus(ClientRoutes.Accounts.InvalidUser,
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.",
                context);
        }

        return user;
    }
}
