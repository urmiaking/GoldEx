using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.StoneTypeAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.StoneTypes;
using GoldEx.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class StoneTypeSeeder(IStoneTypeRepository repository, ILogger<StoneTypeSeeder> logger) : IDbSeeder
{
    public int Order => 80;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var exist = await repository.ExistsAsync(new StoneTypesDefaultSpecification(), cancellationToken);

        if (exist)
            return;

        var stoneTypes = new List<StoneType>
        {
            // =========================
            // Precious (قیمتی)
            // =========================
            StoneType.Create("الماس", "Diamond", "DI", StoneKind.Precious),
            StoneType.Create("یاقوت سرخ", "Ruby", "RU", StoneKind.Precious),
            StoneType.Create("یاقوت کبود", "Sapphire", "SA", StoneKind.Precious),
            StoneType.Create("زمرد", "Emerald", "EM", StoneKind.Precious),

            // =========================
            // Semi-Precious (نیمه‌قیمتی)
            // =========================
            StoneType.Create("آمتیست", "Amethyst", "AM", StoneKind.SemiPrecious),
            StoneType.Create("توپاز", "Topaz", "TO", StoneKind.SemiPrecious),
            StoneType.Create("گارنت", "Garnet", "GA", StoneKind.SemiPrecious),
            StoneType.Create("تورمالین", "Tourmaline", "TU", StoneKind.SemiPrecious),
            StoneType.Create("آکوامارین", "Aquamarine", "AQ", StoneKind.SemiPrecious),
            StoneType.Create("پریدوت", "Peridot", "PE", StoneKind.SemiPrecious),
            StoneType.Create("سیترین", "Citrine", "CI", StoneKind.SemiPrecious),
            StoneType.Create("تانزانیت", "Tanzanite", "TZ", StoneKind.SemiPrecious),
            StoneType.Create("اسپینل", "Spinel", "SP", StoneKind.SemiPrecious),

            // =========================
            // Decorative (تزئینی)
            // =========================
            StoneType.Create("فیروزه", "Turquoise", "TQ", StoneKind.Decorative),
            StoneType.Create("عقیق", "Agate", "AG", StoneKind.Decorative),
            StoneType.Create("اونیکس", "Onyx", "ON", StoneKind.Decorative),
            StoneType.Create("یشم", "Jade", "JA", StoneKind.Decorative),
            StoneType.Create("لاجورد", "Lapis Lazuli", "LL", StoneKind.Decorative),
            StoneType.Create("چشم ببر", "Tiger's Eye", "TE", StoneKind.Decorative),
            StoneType.Create("کهربا", "Amber", "AMR", StoneKind.Decorative),
            StoneType.Create("مرجان", "Coral", "CO", StoneKind.Decorative),
        };

        await repository.CreateRangeAsync(stoneTypes, cancellationToken);

        logger.LogInformation($"{nameof(StoneTypeSeeder)}: Seeded {stoneTypes.Count} stone types.");
    }
}