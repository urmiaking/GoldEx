using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Licenses;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Server.Application.Extensions;
using VHDLicenseManager;
using VHDLicenseManager.Requests;
using VHDLicenseManager.Responses;
using GetLicenseResponse = GoldEx.Shared.DTOs.Licenses.GetLicenseResponse;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class LicenseService(
    ProductLicense productLicense,
    License licenseManager,
    ISettingRepository settingRepository,
    ILogger<LicenseService> logger,
    IWebHostEnvironment environment,
    IFileService fileService,
    IHttpContextAccessor httpContextAccessor,
    ILicenseStore licenseStore,
    RegisterProductRequestValidator validator,
    IStoreContext storeContext,
    ILicenseCache licenseCache,
    IConfiguration configuration) : ILicenseService, IServerLicenseService
{
    public Task<GetLicenseResponse> GetLicenseAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GetLicenseResponse(productLicense.Plan, productLicense.RegisteredAt, productLicense.ExpireDate));
        //return Task.FromResult(new GetLicenseResponse(LicensePlan.Trial, DateTime.Now.AddDays(2)));
    }

    public async Task RegisterProductAsync(RegisterProductRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using var dbTransaction = await settingRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var settings = await settingRepository
                    .Get(new SettingsDefaultSpecification())
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                settings.SetAddress(request.Address);
                settings.SetInstitutionName(request.InstitutionName);
                settings.SetPhoneNumber(request.InstitutionPhoneNumber);

                await settingRepository.UpdateAsync(settings, cancellationToken);

                var isTenant = storeContext.StoreId.HasValue && storeContext.StoreId.Value != Guid.Empty;
                var licenseMode = configuration["License:Mode"] ?? "Hybrid";

                if (isTenant && licenseMode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
                {
                    var localLicenseId = Guid.CreateVersion7();
                    var tenantStoreId = new StoreId(storeContext.StoreId!.Value);

                    var localLicense = AppLicense.Create(
                        localLicenseId,
                        verificationKey: "LOCAL_BYPASS",
                        storeId: tenantStoreId,
                        plan: LicensePlan.Trial,
                        registeredAt: DateTime.Now,
                        expireDate: DateTime.Now.AddDays(14));

                    await licenseStore.SetAsync(localLicense, cancellationToken);
                    licenseCache.Remove(storeContext.StoreId.Value);
                }
                else
                {
                    var newLicenseResponse = await licenseManager.RegisterAsync(nameof(GoldEx),
                        GetDomainAddress(),
                        ApiUrls.Licenses.CallBack(),
                        [],
                        [],
                        request.InstitutionName,
                        request.PhoneNumber,
                        request.Token,
                        cancellationToken);

                    string? errorMessage = null;

                    switch (newLicenseResponse.Result)
                    {
                        case NewLicenseResult.Issued:
                            if (newLicenseResponse.LicenseId is null)
                                throw new Exception("problem at license id issue");
                            break;
                        case NewLicenseResult.InvalidIpAddress:
                        case NewLicenseResult.ProductNotFound:
                        case NewLicenseResult.InvalidPingResponse:
                        case NewLicenseResult.CallbackFailed:
                        case NewLicenseResult.DomainAlreadyRegistered:
                        case NewLicenseResult.IPAlreadyRegistered:
                            errorMessage = "خطایی در فعالسازی محصول رخ داد";
                            break; 
                        case NewLicenseResult.InvalidMobileToken:
                            errorMessage = "کد فعال سازی اشتباه است";
                            break;
                        case NewLicenseResult.InvalidMobile:
                            errorMessage = "شماره همراه وارد شده معتبر نیست";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        logger.LogError(newLicenseResponse.Description);
                        throw new ValidationException(new List<ValidationFailure> { new("", errorMessage) });
                    }

                    var incomingLicense = await licenseManager.GetLicenseAsync(nameof(GoldEx), newLicenseResponse.LicenseId!.Value, cancellationToken);
                    var plan = incomingLicense?.Type.GetLicensePlan() ?? LicensePlan.Trial;
                    var registeredAt = incomingLicense?.RegisteredAt ?? DateTime.Now;
                    var expiry = incomingLicense?.Expiry ?? DateTime.Now.AddDays(14);

                    var appLicense = AppLicense.Create(
                        newLicenseResponse.LicenseId!.Value,
                        verificationKey: null,
                        storeId: default,
                        plan: plan,
                        registeredAt: registeredAt,
                        expireDate: expiry);

                    await licenseStore.SetAsync(appLicense, cancellationToken);
                    licenseCache.Remove(Guid.Empty);
                }

                if (request.IconContent is not null)
                    await fileService.ReplaceLocalFileAsync(environment.GetAppIconPath(storeContext.StoreSlug), request.IconContent, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task SendTokenAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var response = await licenseManager.SendTokenAsync(phoneNumber, cancellationToken);

        if (!response.Sent)
            throw new BadRequestException(response.Description ?? string.Empty);
    }

    public async Task ActivateProductAsync(LicenseCallbackRequest request, CancellationToken cancellationToken = default)
    {
        var activationResponse = await licenseManager.ActivateAsync(request.LicenseId, request.VerificationKey, cancellationToken);

        switch (activationResponse.Result)
        {
            case LicenseActivationResult.Activated:
                var license = await licenseManager.GetLicenseAsync(nameof(GoldEx), request.LicenseId, cancellationToken) 
                              ?? throw new NotFoundException();

                var isTenant = storeContext.StoreId.HasValue && storeContext.StoreId.Value != Guid.Empty;
                var licenseMode = configuration["License:Mode"] ?? "Hybrid";
                var targetStoreGuid = (isTenant && licenseMode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
                    ? storeContext.StoreId!.Value
                    : Guid.Empty;

                var appLicense = await licenseStore.GetAsync(cancellationToken);
                if (appLicense is not null)
                {
                    appLicense.UpdateSubscription(license.Type.GetLicensePlan(), license.RegisteredAt, license.Expiry);
                    if (appLicense.VerificationKey != request.VerificationKey.ToString())
                    {
                        appLicense.UpdateVerificationKey(request.VerificationKey.ToString());
                    }
                    await licenseStore.SetAsync(appLicense, cancellationToken);
                }

                productLicense.UpdateLicense(targetStoreGuid, license.Type.GetLicensePlan(), license.RegisteredAt, license.Expiry);
                licenseCache.Remove(targetStoreGuid);
                break;
            case LicenseActivationResult.InvalidIP:
            case LicenseActivationResult.NotFound:
            case LicenseActivationResult.InvalidVerificationKey:
                throw new Exception(activationResponse.Description);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string GetDomainAddress()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null || !request.Host.HasValue)
            throw new InvalidOperationException();

        var forwardedHost = GetFirstHeader("X-Forwarded-Host");

        var host = !string.IsNullOrWhiteSpace(forwardedHost) ? forwardedHost : request.Host.Value;

        return $"https://{host}";

        string GetFirstHeader(string key)
            => request.Headers.TryGetValue(key, out var values) ? values.ToString().Split(',')[0].Trim() : string.Empty;
    }
}