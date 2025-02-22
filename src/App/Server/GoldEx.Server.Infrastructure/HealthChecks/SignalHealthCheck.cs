using System.Net.Http.Json;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;
using GoldEx.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure;

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
                            "signal apis are unhealthy",
                            $"Signal api responded with {response.StatusCode} and the content of this response is {content}");
                    }
                });
            }

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Signal endpoints is healthy.")
                : HealthCheckResult.Unhealthy("Signal endpoint is unhealthy");
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
                            "signal apis are unhealthy",
                            $"Signal api are unhealthy and has exception: {e.Message}. The Stacktrace is {e.StackTrace}");
                    }
                });
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy($"Signal endpoint, UserManager or EmailSender is unhealthy and has exception: {e.Message}", exception);
            }

            return HealthCheckResult.Unhealthy($"Signal endpoint is unhealthy and has exception: {e.Message}", e);
        }
    }
}