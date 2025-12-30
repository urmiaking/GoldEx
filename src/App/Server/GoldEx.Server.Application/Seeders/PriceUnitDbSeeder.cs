using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal class PriceUnitDbSeeder(IPriceUnitRepository repository, ILogger<PriceUnitDbSeeder> logger) : IDbSeeder
{
    public int Order => 10;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var existingPriceUnits = await repository.Get(new PriceUnitsWithoutSpecification()).ToListAsync(cancellationToken);

        if (existingPriceUnits.Any())
            return;

        var priceUnitsToCreate = Enum.GetValues<UnitType>()
            .Select(unitType => PriceUnit.Create(
                unitType.GetDisplayName(),
                unitType,
                unitType == UnitType.IRR))
            .ToList();

        await repository.CreateRangeAsync(priceUnitsToCreate, cancellationToken);

        logger.LogInformation("Seeded {Count} price units.", priceUnitsToCreate.Count);
    }
}