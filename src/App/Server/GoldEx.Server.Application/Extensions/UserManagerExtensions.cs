using GoldEx.Sdk.Server.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace GoldEx.Server.Application.Extensions;

public static class UserManagerExtensions
{
    public static async Task<IdentityResult> SetFullNameAsync(this UserManager<AppUser> userManager, AppUser user, string fullName)
    {
        user.SetName(fullName);
        return await userManager.UpdateAsync(user);
    }
}