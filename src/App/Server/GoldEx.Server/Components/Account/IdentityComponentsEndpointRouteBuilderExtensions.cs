using System.Security.Claims;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Components.Account.Pages.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Components.Account;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/Logout", async (
            ClaimsPrincipal user,
            SignInManager<AppUser> signInManager,
            [FromForm] string? returnUrl = null) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/{returnUrl ?? ""}");
        }).DisableAntiforgery();

        accountGroup.MapPost("/Login", async (SignInManager<AppUser> signInManager, [FromBody]LoginVm model) =>
        {
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            return result.Succeeded ? Results.Ok() : Results.Forbid();
        });

        return accountGroup;
    }
}