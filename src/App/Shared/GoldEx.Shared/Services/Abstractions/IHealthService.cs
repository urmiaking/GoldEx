using GoldEx.Shared.DTOs.Health;

namespace GoldEx.Shared.Services.Abstractions;

public interface IHealthService
{
    Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default);
}