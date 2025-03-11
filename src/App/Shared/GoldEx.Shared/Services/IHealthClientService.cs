using GoldEx.Shared.DTOs.Health;

namespace GoldEx.Shared.Services;

public interface IHealthClientService
{
    Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default);
}