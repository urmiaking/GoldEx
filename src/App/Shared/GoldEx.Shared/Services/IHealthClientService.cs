using GoldEx.Shared.DTOs;

namespace GoldEx.Shared.Services;

public interface IHealthClientService
{
    Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default);
}