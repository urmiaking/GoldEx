using System.Security.Claims;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Components.Account.Pages;
using GoldEx.Server.Components.Account.Pages.Manage;
using GoldEx.Server.Components.Account.Pages.ViewModels;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace GoldEx.Server.Components.Account;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup(ApiRoutes.Account.Base);

        accountGroup.MapGet($"/{ApiRoutes.Account.Logout}", async (
            SignInManager<AppUser> signInManager,
            [FromQuery] string? returnUrl = null) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/{returnUrl ?? ""}");
        }).DisableAntiforgery();

        accountGroup.MapPost($"/{ApiRoutes.Account.Login}", async (SignInManager<AppUser> signInManager, [FromBody]LoginVm model) =>
        {
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);

            return result.Succeeded ? Results.Ok() : Results.Forbid();
        });

        accountGroup.MapPost($"/{ApiRoutes.Account.PerformExternalLogin}", (
            HttpContext context,
            [FromServices] SignInManager<AppUser> signInManager,
            [FromForm] string provider,
            [FromForm] string returnUrl) =>
        {
            IEnumerable<KeyValuePair<string, StringValues>> query = [
                new KeyValuePair<string, StringValues>("ReturnUrl", returnUrl),
                new KeyValuePair<string, StringValues>("Action", ExternalLogin.LoginCallbackAction)];

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                ClientRoutes.Accounts.ExternalLogin,
                QueryString.Create(query));

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return TypedResults.Challenge(properties, [provider]);
        });

        var manageGroup = accountGroup.MapGroup($"/{ApiRoutes.Account.Manage.Base}").RequireAuthorization();

        manageGroup.MapPost($"/{ApiRoutes.Account.Manage.LinkExternalLogin}", async (
            HttpContext context,
            [FromServices] SignInManager<AppUser> signInManager,
            [FromForm] string provider) =>
        {
            // Clear the existing external cookie to ensure a clean login process
            await context.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                ClientRoutes.Accounts.Manage.ExternalLogins,
                QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, signInManager.UserManager.GetUserId(context.User));
            return TypedResults.Challenge(properties, [provider]);
        });

        return accountGroup;
    }
}