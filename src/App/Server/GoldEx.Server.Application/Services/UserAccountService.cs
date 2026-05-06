using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Extensions;
using GoldEx.Server.Application.Validators.UserAccounts;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class UserAccountService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AccountRequestDtoValidator userAccountValidator,
    IUserStore<AppUser> userStore,
    ILogger<UserAccountService> logger,
    ITransactionContext transactionContext,
    IMemoryCache cache,
    ISmsSender smsSender,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : IUserAccountService
{
    private readonly TimeSpan _cooldown = TimeSpan.FromMinutes(2);

    public async Task<GetUserAccountResponse> GetCurrentUserInfoAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        var user = await userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetUserAccountResponse>(user);
    }

    public async Task UpdateUserFullNameAsync(UpdateUserFullNameRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        await userManager.SetFullNameAsync(user, request.FullName);

        await signInManager.RefreshSignInAsync(user);
    }

    public async Task UpdateUserPhoneNumberAsync(UpdateUserPhoneNumberRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var isValid = await userManager.VerifyChangePhoneNumberTokenAsync(user, request.Token, request.NewPhoneNumber);

        if (!isValid)
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewPhoneNumber), "کد تایید وارد شده اشتباه است")
            ]);

        var result = await userManager.ChangePhoneNumberAsync(user, request.NewPhoneNumber, request.Token);

        if (!result.Succeeded)
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewPhoneNumber), string.Join(",", result.Errors.Select(x => x.Description)))
            ]);

        await signInManager.RefreshSignInAsync(user);
    }

    public async Task SendVerificationTokenAsync(SendVerificationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"phone:smsCooldown:{request.OldPhoneNumber}";

        if (cache.TryGetValue(cacheKey, out _))
        {
#if !DEBUG
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewPhoneNumber),
                    $"لطفاً بعد از {_cooldown.TotalMinutes} دقیقه دوباره تلاش کنید")
            ]);
#endif
        }

        if (!userManager.SupportsUserPhoneNumber)
            throw new ValidationException([new ValidationFailure(nameof(request.NewPhoneNumber), "این عملیات قابل اجرا نمی باشد")]);

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.OldPhoneNumber, cancellationToken) ??
                   throw new NotFoundException();

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, request.NewPhoneNumber);

        var sent = await smsSender.SendAsync(request.OldPhoneNumber, $"کد تایید: {code}", cancellationToken);

        if (!sent)
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewPhoneNumber), "خطا در ارسال کد تایید")
            ]);

        cache.Set(cacheKey, true, _cooldown);
    }

    public async Task UpdateUserEmailAsync(UpdateUserEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var result = await userManager.SetEmailAsync(user, request.NewEmail);

        if (!result.Succeeded)
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewEmail), string.Join(",", result.Errors.Select(x => x.Description)))
            ]);
    }

    public async Task UpdateUserPasswordAsync(UpdateUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var passwordValid = await userManager.CheckPasswordAsync(user, request.OldPassword);

        if (!passwordValid)
            throw new ValidationException([
                new ValidationFailure(nameof(request.OldPassword), "رمز عبور فعلی اشتباه است")
            ]);

        var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
            throw new ValidationException([
                new ValidationFailure(nameof(request.NewPassword), string.Join(",", result.Errors.Select(x => x.Description)))
            ]);

        await signInManager.RefreshSignInAsync(user);
    }

    public async Task<GetTwoFactorAuthStatusResponse> GetUser2FaStatusAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var is2FaEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        var isMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
        var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);

        return new GetTwoFactorAuthStatusResponse(is2FaEnabled, isMachineRemembered, recoveryCodesLeft);
    }

    public async Task ForgetDeviceAsync(CancellationToken cancellationToken = default)
    {
        await signInManager.ForgetTwoFactorClientAsync();
    }

    public async Task Disable2FaAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var disable2FaResult = await userManager.SetTwoFactorEnabledAsync(user, false);

        if (!disable2FaResult.Succeeded)
            throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
    }

    public async Task<GetAuthenticatorKeyResponse> GetAuthenticatorKeyAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(authenticatorKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        return new GetAuthenticatorKeyResponse(authenticatorKey!);
    }

    public async Task<EnableTwoFactorAuthResponse> Enable2FaAsync(EnableTwoFactorAuthRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var is2FaTokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Token);

        if (!is2FaTokenValid)
            throw new ValidationException([new ValidationFailure(nameof(request.Token), "کد وارد شده معتبر نیست")]);

        await userManager.SetTwoFactorEnabledAsync(user, true);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return new EnableTwoFactorAuthResponse(recoveryCodes!);
    }

    public async Task<GetExternalProviderResponse> GetExternalProvidersAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var currentLogins = await userManager.GetLoginsAsync(user);
        var otherLogins = await signInManager.GetExternalAuthenticationSchemesAsync();

        string? passwordHash = null;
        if (userStore is IUserPasswordStore<AppUser> userPasswordStore)
        {
            passwordHash = await userPasswordStore.GetPasswordHashAsync(user, cancellationToken);
        }

        var showRemoveButton = passwordHash is not null || currentLogins.Count > 1;

        var currentLoginsDto = currentLogins.Select(x => new LinkedLoginDto(x.LoginProvider,
            x.ProviderDisplayName,
            x.ProviderKey)).ToList();

        var otherLoginsDto = otherLogins.Select(x => new ExternalProviderDto(x.Name,
            x.DisplayName)).ToList();

        return new GetExternalProviderResponse(currentLoginsDto, otherLoginsDto, showRemoveButton);
    }

    public async Task<List<GetPasskeyResponse>> GetPasskeysAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var passkeys = await userManager.GetPasskeysAsync(user);

        return mapper.Map<List<GetPasskeyResponse>>(passkeys);
    }

    public async Task<string> GetPasskeyCreationOptionsAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var userId = await userManager.GetUserIdAsync(user);
        var userName = await userManager.GetUserNameAsync(user) ?? "User";

        var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
        {
            Id = userId,
            Name = userName,
            DisplayName = userName
        });

        return optionsJson;
    }

    public async Task<string> GetPasskeyRequestOptionsAsync(string? userName, CancellationToken cancellationToken = default)
    {
        var user = string.IsNullOrEmpty(userName) ? null : await userManager.FindByNameAsync(userName);

        var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);

        return optionsJson;
    }

    public async Task AddPasskeyAsync(CreatePasskeyRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var attestationResult = await signInManager.PerformPasskeyAttestationAsync(request.CredentialJson);

        if (!attestationResult.Succeeded)
            throw new InvalidOperationException(attestationResult.Failure?.Message);

        var httpContext = httpContextAccessor.HttpContext;
        var deviceName = httpContext?.GetDeviceName();
        attestationResult.Passkey.Name = deviceName;

        var result = await userManager.AddOrUpdatePasskeyAsync(user, attestationResult.Passkey);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
    }

    public async Task RemovePasskeyAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(cancellationToken);

        var result = await userManager.RemovePasskeyAsync(user, WebEncoders.Base64UrlDecode(credentialId));

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
    }

    public async Task<List<GetUserAccountResponse>> GetAccountsListAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .OrderBy(x => x.UserName)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetUserAccountResponse>>(users);
    }

    public async Task LockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException();

        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
    }

    public async Task UnlockUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException();

        var result = await userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
    }

    public async Task CreateAccountAsync(UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await userAccountValidator.ValidateAndThrowAsync(request, cancellationToken);

        await using var dbTransaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        {
            try
            {
                var user = new AppUser(request.FullName, request.Username, request.Email, request.PhoneNumber);

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));

                var passwordResult = await userManager.AddPasswordAsync(user, request.Password!);

                if (!passwordResult.Succeeded)
                    throw new InvalidOperationException(string.Join(",", passwordResult.Errors.Select(x => x.Description)));

                await userManager.AddToRoleAsync(user, request.Role);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                logger.LogError(e, e.Message);
                throw;
            }
        }
    }

    public async Task UpdateAccountAsync(Guid id, UserAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        if (id != request.Id)
            throw new BadRequestException();

        await userAccountValidator.ValidateAndThrowAsync(request, cancellationToken);

        await using var dbTransaction = await transactionContext.BeginTransactionAsync(cancellationToken);
        {
            try
            {
                var user = await userManager.Users
                    .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                           ?? throw new NotFoundException();

                if (!string.Equals(user.Name, request.FullName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var result = await userManager.SetFullNameAsync(user, request.FullName);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
                }

                if (!string.Equals(user.UserName, request.Username, StringComparison.CurrentCultureIgnoreCase))
                {
                    var result = await userManager.SetUserNameAsync(user, request.Username);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
                }

                if (!string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.CurrentCultureIgnoreCase))
                {
                    var result = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
                }

                if (!string.Equals(user.Email, request.Email, StringComparison.CurrentCultureIgnoreCase))
                {
                    var result = await userManager.SetEmailAsync(user, request.Email);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(string.Join(",", result.Errors.Select(x => x.Description)));
                }

                if (!string.IsNullOrEmpty(request.Password))
                {
                    await userManager.RemovePasswordAsync(user);
                    var passwordResult = await userManager.AddPasswordAsync(user, request.Password);

                    if (!passwordResult.Succeeded)
                        throw new InvalidOperationException(string.Join(",", passwordResult.Errors.Select(x => x.Description)));
                }

                if (user.UserRoles.All(x => x.Role.Name != request.Role))
                {
                    await userManager.RemoveFromRoleAsync(user, user.UserRoles.FirstOrDefault()!.Role.Name!);
                    await userManager.AddToRoleAsync(user, request.Role);
                }

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                logger.LogError(e, e.Message);
                throw;
            }
        }
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException();
        await userManager.DeleteAsync(user);
    }

    private async Task<AppUser> GetRequiredUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.GetUserId() ?? throw new UnauthorizedAccessException();

        return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) 
               ?? throw new NotFoundException();
    }
}