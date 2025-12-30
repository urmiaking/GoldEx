using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class PriceCatalogDbSeeder(
    IPriceRepository priceRepository,
    IPriceUnitRepository priceUnitRepository,
    ILogger<PriceCatalogDbSeeder> logger
) : IDbSeeder
{
    public int Order => 20;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken)
    {
        var newPrices = await MigrateAndSeedPriceCatalogsAsync(
            priceRepository,
            priceUnitRepository,
            logger,
            cancellationToken);

        context.Set(SeedContextKeys.NewlyCreatedPrices, newPrices);

        logger.LogInformation($"{nameof(PriceCatalogDbSeeder)}: Seeded price catalogs.");
    }

    private static async Task<List<Price>> MigrateAndSeedPriceCatalogsAsync(IPriceRepository priceRepository, IPriceUnitRepository priceUnitRepository, ILogger<PriceCatalogDbSeeder> logger, CancellationToken cancellationToken = default)
    {
        // 1. Load all existing prices
        var existingPrices = await priceRepository
            .Get(new PricesWithoutSpecification())
            .ToListAsync(cancellationToken);

        // 2. Migrate: assign PriceCatalog to existing rows that don’t have it (or were default)
        // Define what "unassigned" means. If the enum is non-nullable and default(PriceCatalog) is a valid value,
        // you may need a sentinel or check Title mismatch. Adjust below accordingly.
        var migratedPrices = new List<Price>();
        foreach (var price in existingPrices)
        {
            var hasCatalog = price.PriceCatalog != default; // adjust if default is valid
            if (hasCatalog)
                continue;

            // Try map by title
            if (PriceCatalogHelper.TryGetByTitle(price.Title, out var catalog))
            {
                price.SetCatalog(catalog);
                migratedPrices.Add(price);
            }
            else
            {
                // If mapping fails, log & skip (will not duplicate later because we won't seed anything with same Title)
                logger.LogWarning("Could not map existing Price '{Title}' to any PriceCatalog.", price.Title);
            }
        }

        if (migratedPrices.Count > 0)
        {
            // Persist migration
            await priceRepository.UpdateRangeAsync(migratedPrices, cancellationToken);
            logger.LogInformation("Migrated {Count} existing prices to PriceCatalog.", migratedPrices.Count);
        }

        // 3. Recompute existing catalogs after migration
        var existingCatalogSet = existingPrices
            .Where(p => p.PriceCatalog != default)
            .Select(p => p.PriceCatalog)
            .ToHashSet();

        // 4. Determine catalogs still missing
        var allCatalogs = Enum.GetValues<PriceCatalog>();
        var catalogsToCreate = allCatalogs
            .Where(c => !existingCatalogSet.Contains(c))
            .ToList();

        if (catalogsToCreate.Count == 0)
            return [];

        // 5. Create new Prices for missing catalogs
        var newlyCreatedPrices = new List<Price>();
        foreach (var catalog in catalogsToCreate)
        {
            var newPrice = Price.Create(
                catalog
            );
            newlyCreatedPrices.Add(newPrice);
        }

        if (newlyCreatedPrices.Count > 0)
        {
            await priceRepository.CreateRangeAsync(newlyCreatedPrices, cancellationToken);
            logger.LogInformation("Seeded {Count} new prices from missing PriceCatalog entries.", newlyCreatedPrices.Count);
        }

        // 5. Remove (or disable) invalid PriceUnits (not in UnitType enum) before linking
        {
            var allPriceUnitsQuery = priceUnitRepository.Get(new PriceUnitsWithoutSpecification());

            var validUnitTypes = Enum.GetValues(typeof(UnitType))
                .Cast<int>()
                .ToList();

            var invalidUnits = await allPriceUnitsQuery
                .Where(pu => !pu.UnitType.HasValue || !validUnitTypes.Contains((int)pu.UnitType.Value))
                .ToListAsync(cancellationToken);


            if (invalidUnits.Count > 0)
            {
                // Choose one option:

                // OPTION A: Hard delete (ensure no FK references!)
                await priceUnitRepository.DeleteRangeAsync(invalidUnits, cancellationToken);

                // OPTION B: Disable (safer)
                //foreach (var pu in invalidUnits)
                //    pu.SetStatus(false);

                //await priceUnitRepository.UpdateRangeAsync(invalidUnits, cancellationToken);

                logger.LogInformation("Handled {Count} invalid PriceUnits (not in UnitType enum).", invalidUnits.Count);
            }
        }

        // 6. Link PriceUnits without PriceId
        var priceUnitsWithoutPrice = await priceUnitRepository
            .Get(new PriceUnitsWithoutPriceIdSpecification())
            .ToListAsync(cancellationToken);

        if (priceUnitsWithoutPrice.Count > 0)
        {
            // Refresh prices (include newly created)
            var allPrices = await priceRepository
                .Get(new PricesWithoutSpecification())
                .ToListAsync(cancellationToken);

            var priceByTitle = allPrices.ToDictionary(p => p.Title, p => p.Id, StringComparer.Ordinal);
            var dirtyUnits = new List<PriceUnit>();

            foreach (var pu in priceUnitsWithoutPrice)
            {
                if (priceByTitle.TryGetValue(pu.Title, out var foundId) && pu.PriceId != foundId)
                {
                    pu.SetPriceId(foundId);
                    dirtyUnits.Add(pu);
                }
            }

            if (dirtyUnits.Count > 0)
            {
                await priceUnitRepository.UpdateRangeAsync(dirtyUnits, cancellationToken);
                logger.LogInformation("Linked {Count} PriceUnits to their Prices.", dirtyUnits.Count);
            }
        }

        // Return only newly created prices (for any follow‑up logic)
        return newlyCreatedPrices;
    }
}