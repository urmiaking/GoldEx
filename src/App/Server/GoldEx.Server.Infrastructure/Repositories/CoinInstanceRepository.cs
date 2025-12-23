using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class CoinInstanceRepository(GoldExDbContext dbContext) : RepositoryBase<CoinInstance>(dbContext), ICoinInstanceRepository
{
    public async Task<string?> GetLastBarcodeAsync(CancellationToken cancellationToken)
    {
        return await Query
            .OrderByDescending(ci => ci.Barcode)
            .Select(ci => ci.Barcode)
            .FirstOrDefaultAsync(cancellationToken);
    }
}