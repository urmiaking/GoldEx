using GoldEx.Calculator.Server.Models;
using GoldEx.Sdk.Server.Api;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Calculator.Server.Controllers;

[AllowAnonymous]
[Route(ApiRoutes.Account.Base)]
public class AccountsController(SignInManager<AppUser> signInManager) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Account.Logout)]
    [IgnoreAntiforgeryToken]
    public async Task<IResult> Logout([FromQuery] string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return TypedResults.LocalRedirect($"~/{returnUrl ?? ""}");
    }

    [HttpPost(ApiRoutes.Account.Login)]
    public async Task<IResult> Login([FromBody] LoginVm model)
    {
        var result = await signInManager.PasswordSignInAsync(
            model.Username,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        return result.Succeeded
            ? Results.Ok()
            : Results.Forbid();
    }
}