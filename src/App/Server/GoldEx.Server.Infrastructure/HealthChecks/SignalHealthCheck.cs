using System.Net.Http.Json;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;
using GoldEx.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GoldEx.Shared.Routings;

namespace GoldEx.Server.Infrastructure.HealthChecks;

public class SignalHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            var payload = SignalPayloadItem.CreateDefaultPayload();
            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, Utilities.GetJsonOptions(), cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Signal endpoints is healthy.")
                : HealthCheckResult.Unhealthy("Signal endpoint is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy($"Signal endpoint is unhealthy and has exception: {e.Message}", e);
        }
    }
}