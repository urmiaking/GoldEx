using GoldEx.Shared.DTOs.CoinInstances;

namespace GoldEx.Shared.Services.Abstractions;

public interface ICoinInstanceService
{
    Task<GetCoinInstanceResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default);
}