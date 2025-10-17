using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;
using GoldEx.Shared;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Json;

namespace GoldEx.Server.Infrastructure.HealthChecks;

public class SignalHealthCheck(IHttpClientFactory httpClientFactory, IEmailSender emailSender, UserManager<AppUser> userManager) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            var payload = SignalPayloadItem.CreateDefaultPayload();
            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, Utilities.GetJsonOptions(), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                userManager.GetUsersInRoleAsync(BuiltinRoles.Administrators).Result.ToList().ForEach(async user =>
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        await emailSender.SendEmailAsync(user.Email,
                            "سرویس استعلام قیمت آنلاین با مشکل مواجه شده است",
                            $"کد خطا: {response.StatusCode} و متن خطا: {content}");
                    }
                });
            }

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("اختلال در سرویس");
        }
        catch (Exception e)
        {
            try
            {
                userManager.GetUsersInRoleAsync(BuiltinRoles.Administrators).Result.ToList().ForEach(async user =>
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        await emailSender.SendEmailAsync(user.Email,
                            "سرویس استعلام قیمت آنلاین با مشکل مواجه شده است",
                            $"سرویس استعلام قیمت آنلاین با مشکل مواجه شده است و خطا: {e.Message}. Stacktrace: {e.StackTrace}");
                    }
                });
            }
            catch 
            {
                return HealthCheckResult.Unhealthy("اختلال در سرویس");
            }

            return HealthCheckResult.Unhealthy("اختلال در سرویس");
        }
    }
}