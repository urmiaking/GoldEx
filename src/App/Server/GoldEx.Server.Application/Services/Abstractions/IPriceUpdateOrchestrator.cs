namespace GoldEx.Server.Application.Services.Abstractions;

public interface IPriceUpdateOrchestrator
{
    Task UpdateAllAsync(CancellationToken cancellationToken = default);
}