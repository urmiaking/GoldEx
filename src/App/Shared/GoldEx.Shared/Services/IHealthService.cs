using GoldEx.Shared.DTOs.Health;

namespace GoldEx.Shared.Services;

public interface IHealthService
{
    Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default);
}