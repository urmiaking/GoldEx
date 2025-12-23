using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinInstanceAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICoinInstanceRepository : IRepository<CoinInstance>,
    ICreateRepository<CoinInstance>,
    IUpdateRepository<CoinInstance>,
    IDeleteRepository<CoinInstance>
{
    Task<string?> GetLastBarcodeAsync(CancellationToken cancellationToken);
}