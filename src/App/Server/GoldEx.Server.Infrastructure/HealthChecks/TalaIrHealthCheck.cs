using Microsoft.Extensions.Diagnostics.HealthChecks;
using GoldEx.Shared.Routings;

namespace GoldEx.Server.Infrastructure.HealthChecks;

public class TalaIrHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Tala.ir endpoints is healthy.")
                : HealthCheckResult.Unhealthy("Tala.ir endpoint is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Tala.ir endpoint is unhealthy and has exception: ", e);
        }
        
    }
}