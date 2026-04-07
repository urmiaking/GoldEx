using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Api;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.UserAccounts.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class UserAccountsController(
    IUserAccountService service,
    IUserContext userContext,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager) : ApiControllerBase
{
    [HttpGet(ApiRoutes.UserAccounts.GetCurrentUser)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userInfo = await service.GetCurrentUserInfoAsync(cancellationToken);
        return Ok(userInfo);
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdateFullName)]
    public async Task<IActionResult> UpdateFullNameAsync([FromBody] UpdateUserFullNameRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserFullNameAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdatePhoneNumber)]
    public async Task<IActionResult> UpdatePhoneNumberAsync([FromBody] UpdateUserPhoneNumberRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserPhoneNumberAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost(ApiRoutes.UserAccounts.SendVerificationToken)]
    public async Task<IActionResult> SendVerificationTokenAsync(SendVerificationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.SendVerificationTokenAsync(request, cancellationToken);
        return Ok();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdateEmail)]
    public async Task<IActionResult> UpdateEmailAsync([FromBody] UpdateUserEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserEmailAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut(ApiRoutes.UserAccounts.UpdatePassword)]
    public async Task<IActionResult> UpdatePasswordAsync([FromBody] UpdateUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        await service.UpdateUserPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet(ApiRoutes.UserAccounts.Get2FaStatus)]
    public async Task<IActionResult> Get2FaStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = await service.GetUser2FaStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost(ApiRoutes.UserAccounts.ForgetDevice)]
    public async Task<IActionResult> ForgetDeviceAsync(CancellationToken cancellationToken = default)
    {
        await service.ForgetDeviceAsync(cancellationToken);
        return Ok();
    }

    [HttpPost(ApiRoutes.UserAccounts.Disable2Fa)]
    public async Task<IActionResult> Disable2FaAsync(CancellationToken cancellationToken = default)
    {
        await service.Disable2FaAsync(cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.UserAccounts.GetAuthenticatorKey)]
    public async Task<IActionResult> GetAuthenticatorKeyAsync(CancellationToken cancellationToken = default)
    {
        var key = await service.GetAuthenticatorKeyAsync(cancellationToken);
        return Ok(key);
    }

    [HttpPost(ApiRoutes.UserAccounts.Enable2Fa)]
    public async Task<IActionResult> Enable2FaAsync(EnableTwoFactorAuthRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.Enable2FaAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.UserAccounts.GetExternalProviders)]
    public async Task<IActionResult> GetExternalProvidersAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetExternalProvidersAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet(ApiRoutes.UserAccounts.ExternalLogin)]
    public IActionResult ExternalLogin([FromQuery] string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "UserAccounts", new { returnUrl });
        var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(props, provider);
    }

    [HttpGet(ApiRoutes.UserAccounts.ExternalLoginCallback)]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info == null)
            return Redirect(ClientRoutes.UserAccounts.ExternalLogins.AppendQueryString(new { result = ExternalLoginResult.NoInfo }));

        var user = await GetUserAsync();
        await userManager.AddLoginAsync(user, info);

        return Redirect(ClientRoutes.UserAccounts.ExternalLogins.AppendQueryString(new { result = ExternalLoginResult.Success }));
    }

    [HttpGet(ApiRoutes.UserAccounts.GetPasskeys)]
    public async Task<IActionResult> GetPasskeysAsync(CancellationToken cancellationToken = default)
    {
        var list = await service.GetPasskeysAsync(cancellationToken);
        return Ok(list);
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpGet(ApiRoutes.UserAccounts.GetPasskeyRequestOptions)]
    public async Task<IActionResult> GetPasskeyRequestOptionsAsync([FromQuery] string? username, CancellationToken cancellationToken = default)
    {
        var result = await service.GetPasskeyRequestOptionsAsync(username, cancellationToken);
        return Ok(result);
    }

    [IgnoreAntiforgeryToken]
    [HttpGet(ApiRoutes.UserAccounts.GetPasskeyCreationOptions)]
    public async Task<IActionResult> GetPasskeyCreationOptionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await service.GetPasskeyCreationOptionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.UserAccounts.AddPasskey)]
    public async Task<IActionResult> AddPasskeyAsync(CreatePasskeyRequest request, CancellationToken cancellationToken = default)
    {
        await service.AddPasskeyAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpDelete(ApiRoutes.UserAccounts.RemovePasskey)]
    public async Task<IActionResult> RemovePasskeyAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        await service.RemovePasskeyAsync(credentialId, cancellationToken);
        return NoContent();
    }

    private async Task<AppUser> GetUserAsync()
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedException();
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user ?? throw new NotFoundException();
    }
}