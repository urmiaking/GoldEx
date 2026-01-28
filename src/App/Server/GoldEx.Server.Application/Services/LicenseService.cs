using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Licenses;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Shared.Enums;
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
    RegisterProductRequestValidator validator) : ILicenseService, IServerLicenseService
{
    public Task<GetLicenseResponse> GetLicenseAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GetLicenseResponse(productLicense.Plan, productLicense.ExpireDate));
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

                await settingRepository.UpdateAsync(settings, cancellationToken);

                var newLicenseResponse = await licenseManager.RegisterAsync("GoldEx",
                    GetDomainAddress(),
                    ApiRoutes.Licenses.CallbackUrl,
                    [],
                    [],
                    request.InstitutionName,
                    request.PhoneNumber,
                    request.Token.ToString(),
                    cancellationToken);

                string? errorMessage = null;

                switch (newLicenseResponse.Result)
                {
                    case NewLicenseResult.Issued:
                        break;
                    case NewLicenseResult.InvalidIpAddress:
                    case NewLicenseResult.ProductNotFound:
                    case NewLicenseResult.InvalidPingResponse:
                    case NewLicenseResult.CallbackFailed:
                        errorMessage = "خطایی در فعالسازی محصول رخ داد";
                        break;
                    case NewLicenseResult.DomainAlreadyRegistered:
                    case NewLicenseResult.IPAlreadyRegistered:
                        // we may need to ignore it
                        break;
                    case NewLicenseResult.InvalidMobileToken:
                        errorMessage = "کد فعال سازی اشتباه است";
                        break;
                    case NewLicenseResult.InvalidMobile:
                        // invalid mobile? wait what?
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new ValidationException(new List<ValidationFailure> { new("", errorMessage) });

                if (request.IconContent is not null)
                    await fileService.ReplaceLocalFileAsync(environment.GetAppIconPath(), request.IconContent, cancellationToken);

                // TODO: remove this
                productLicense.UpdateLicense(LicensePlan.Trial, DateTime.Today.AddDays(30));

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
        await licenseManager.SendTokenAsync(phoneNumber, cancellationToken);
    }

    public async Task ActivateProductAsync(LicenseCallbackRequest request, CancellationToken cancellationToken = default)
    {
        var activationResponse = await licenseManager.ActivateAsync(request.LicenseId, request.VerificationKey, cancellationToken);

        switch (activationResponse.Result)
        {
            case LicenseActivationResult.Activated:
                productLicense.UpdateLicense(LicensePlan.Trial, DateTime.Today.AddDays(30));
                break;
            case LicenseActivationResult.InvalidIP:
            case LicenseActivationResult.NotFound:
            case LicenseActivationResult.InvalidVerificationKey:
                throw new Exception();
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