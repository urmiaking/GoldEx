using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GoldEx.Server.Extensions;

public static class ServiceProviderExtensions
{
    internal static async Task MigrateAsync(this IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<GoldExDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
            await dbContext.Database.MigrateAsync();
    }

    internal static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        await EnsureDatabasePopulated(serviceProvider);
    }

    private static async Task EnsureDatabasePopulated(IServiceProvider serviceProvider)
    {
        await PopulateDefaultSettingsAsync(serviceProvider);
        await PopulateDefaultPriceUnitsAsync(serviceProvider);
        var newPrices = await MigrateAndSeedPriceCatalogsAsync(serviceProvider);
        await PopulateDefaultLedgerAccountsAsync(serviceProvider);

        var coinPrices = newPrices.Where(x => x.MarketType is MarketType.Coin).ToList();

        await PopulateDefaultCoinsAsync(coinPrices, serviceProvider);
        await EnsureSystemFinancialAccountsExistAsync(serviceProvider);

        await PopulateDefaultProductCategoriesAsync(serviceProvider);

        var accountService = serviceProvider.GetRequiredService<IAccountService>();
        var policyProviders = serviceProvider.GetServices<IApplicationPolicyProvider>();

        foreach (var item in BuiltinRoles.Roles)
            await accountService.EnsureRoleAsync(item);

        await PopulateAdministratorClaimsAsync(policyProviders, accountService);

        var admin = await accountService.FindUserAsync("admin");

        if (admin is null)
        {
            var adminUser = serviceProvider.GetRequiredService<IOptions<UserSetting>>().Value;

            await accountService.CreateUserAsync(new AppUser("مدیر سامانه", adminUser.UserName, adminUser.Email, adminUser.PhoneNumber),
                adminUser.Password, [BuiltinRoles.Administrators]);
        }
    }

    private static async Task EnsureSystemFinancialAccountsExistAsync(IServiceProvider serviceProvider)
    {
        await EnsureGoldAccountExistsAsync(serviceProvider);
        await EnsureCashAccountExistsAsync(serviceProvider);
    }

    private static async Task EnsureGoldAccountExistsAsync(IServiceProvider serviceProvider)
    {
        var financialAccountRepository = serviceProvider.GetRequiredService<IFinancialAccountRepository>();
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();
        var ledgerAccountRepository = serviceProvider.GetRequiredService<ILedgerAccountRepository>();

        if (await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Gold)))
            return;

        var goldPriceUnit = await priceUnitRepository.Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Gold price unit is not initialized");

        var inventoryLedgerAccount = await ledgerAccountRepository.Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Molten Gold Inventory ledger account is not initialized");

        var goldAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Gold, goldPriceUnit.Id, inventoryLedgerAccount.Id);
        await financialAccountRepository.CreateAsync(goldAccount);
    }

    private static async Task EnsureCashAccountExistsAsync(IServiceProvider serviceProvider)
    {
        var financialAccountRepository = serviceProvider.GetRequiredService<IFinancialAccountRepository>();
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();
        var ledgerAccountRepository = serviceProvider.GetRequiredService<ILedgerAccountRepository>();

        if (await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Cash)))
            return;

        var tomanPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByTitleSpecification(UnitType.Toman.GetDisplayName()))
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Toman price unit is not initialized");

        var internalCashLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.InternalCashAccounts))
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Internal Cash Accounts ledger account is not initialized");

        var cashAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Cash, tomanPriceUnit.Id, internalCashLedgerAccount.Id,
            cashAccount: CashAccount.Create(null, CashAccountType.Internal));

        await financialAccountRepository.CreateAsync(cashAccount);
    }

    private static async Task PopulateDefaultCoinsAsync(List<Price> coinPrices, IServiceProvider serviceProvider)
    {
        var coinService = serviceProvider.GetRequiredService<ICoinService>();

        var existingCoins = await coinService.GetListAsync(null);

        var newCoins = coinPrices
            .Where(price => existingCoins.All(c => c.PriceId != price.Id.Value))
            .Select(price => new CoinRequestDto(null, price.Title, price.Id.Value))
            .ToList();

        foreach (var newCoin in newCoins) 
            await coinService.CreateAsync(newCoin);
    }

    private static async Task<List<Price>> MigrateAndSeedPriceCatalogsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var priceRepository = serviceProvider.GetRequiredService<IPriceRepository>();
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PriceCatalogSeeder");

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
        {
            logger.LogInformation("No new catalogs to seed. All PriceCatalog values already represented.");
        }

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

    private static async Task PopulateDefaultLedgerAccountsAsync(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<ILedgerAccountRepository>();
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();

        // متد کمکی برای جلوگیری از تکرار کد
        async Task<LedgerAccount> GetOrCreateAccount(string title, LedgerAccountType type, LedgerAccount? parent = null, UnitType? unitType = null)
        {
            var existingAccount = await repository
                .Get(new LedgerAccountsByTitleSpecification(title))
                .Include(x => x.PriceUnit)
                .FirstOrDefaultAsync();

            if (existingAccount is not null)
            {
                if (unitType.HasValue && existingAccount.PriceUnit?.UnitType != unitType)
                {
                    var priceUnit = await priceUnitRepository.Get(new PriceUnitsByUnitTypeSpecification(unitType.Value))
                        .FirstOrDefaultAsync();

                    existingAccount.SetPriceUnitId(priceUnit?.Id);

                    await repository.UpdateAsync(existingAccount);
                }

                return existingAccount;
            }

            PriceUnitId? priceUnitId = null;

            if (unitType.HasValue)
            {
                var priceUnit = await priceUnitRepository.Get(new PriceUnitsByUnitTypeSpecification(unitType.Value))
                    .FirstOrDefaultAsync();

                if (priceUnit is not null)
                    priceUnitId = priceUnit.Id;
            }

            var newAccount = LedgerAccount.CreateSystemAccount(title, type, parent?.Id, priceUnitId);
            await repository.CreateAsync(newAccount);
            return newAccount;
        }

        // --- ایجاد سلسله مراتبی سرفصل‌ها ---

        // سطح ۱ (اصلی)
        var assets = await GetOrCreateAccount(SystemLedgerAccounts.Assets, LedgerAccountType.Asset);
        var liabilities = await GetOrCreateAccount(SystemLedgerAccounts.Liabilities, LedgerAccountType.Liability);
        var equity = await GetOrCreateAccount(SystemLedgerAccounts.Equity, LedgerAccountType.Equity);
        var revenue = await GetOrCreateAccount(SystemLedgerAccounts.Revenue, LedgerAccountType.Revenue);
        var expenses = await GetOrCreateAccount(SystemLedgerAccounts.Expenses, LedgerAccountType.Expense);

        // سطح ۲ (زیرمجموعه‌ها)
        var currentAssets = await GetOrCreateAccount(SystemLedgerAccounts.CurrentAssets, LedgerAccountType.Asset, assets);
        var currentLiabilities = await GetOrCreateAccount(SystemLedgerAccounts.CurrentLiabilities, LedgerAccountType.Liability, liabilities);
        var operatingExpenses = await GetOrCreateAccount(SystemLedgerAccounts.OperatingExpenses, LedgerAccountType.Expense, expenses);
        var openingBalanceEquity = await GetOrCreateAccount(SystemLedgerAccounts.OpeningBalanceEquity, LedgerAccountType.Equity, equity);

        // سطح ۳ (زیرمجموعه‌های نهایی)
        await GetOrCreateAccount(SystemLedgerAccounts.AccountsReceivable, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.PrepaymentsToSuppliers, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.Inventory, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.UsedProductInventory, LedgerAccountType.Asset, currentAssets, UnitType.Gold18K);
        await GetOrCreateAccount(SystemLedgerAccounts.CoinInventory, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.MoltenGoldInventory, LedgerAccountType.Asset, currentAssets, UnitType.Gold18K);
        await GetOrCreateAccount(SystemLedgerAccounts.Banks, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.DepositsWithOthers, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.LoansToOthers, LedgerAccountType.Asset, currentAssets);

        var cashAccounts = await GetOrCreateAccount(SystemLedgerAccounts.CashAccounts, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.InternalCashAccounts, LedgerAccountType.Asset, cashAccounts);

        await GetOrCreateAccount(SystemLedgerAccounts.AccountsPayable, LedgerAccountType.Liability, currentLiabilities);

        await GetOrCreateAccount(SystemLedgerAccounts.SalesRevenue, LedgerAccountType.Revenue, revenue);
        await GetOrCreateAccount(SystemLedgerAccounts.AdditionalChargesRevenue, LedgerAccountType.Revenue, revenue);
        await GetOrCreateAccount(SystemLedgerAccounts.ExchangeGainLoss, LedgerAccountType.Revenue, revenue);

        await GetOrCreateAccount(SystemLedgerAccounts.CostOfGoodsSold, LedgerAccountType.Expense, expenses);
        await GetOrCreateAccount(SystemLedgerAccounts.SalesDiscounts, LedgerAccountType.Expense, expenses);
        await GetOrCreateAccount(SystemLedgerAccounts.PurchaseDiscounts, LedgerAccountType.Expense, expenses);
        await GetOrCreateAccount(SystemLedgerAccounts.ServiceExpenses, LedgerAccountType.Expense, operatingExpenses);
        await GetOrCreateAccount(SystemLedgerAccounts.PurchaseOverheads, LedgerAccountType.Expense, expenses);

        await GetOrCreateAccount(SystemLedgerAccounts.OwnerDraw, LedgerAccountType.Equity, equity);
    }

    private static async Task PopulateDefaultProductCategoriesAsync(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IProductCategoryRepository>();

        var productCategoryCount = await repository.CountAsync(new ProductCategoriesDefaultSpecification());

        if (productCategoryCount > 0)
            return;

        var defaultCategories = new List<ProductCategory>
        {
            ProductCategory.Create("انگشتر", "01"),
            ProductCategory.Create("النگو", "02"),
            ProductCategory.Create("دستبند", "03"),
            ProductCategory.Create("گوشواره", "04"),
            ProductCategory.Create("سرویس کامل", "05"),
            ProductCategory.Create("گردنبند", "06"),
            ProductCategory.Create("نیم‌ست", "07")
        };

        await repository.CreateRangeAsync(defaultCategories);
    }

    private static async Task PopulateDefaultPriceUnitsAsync(IServiceProvider serviceProvider)
    {
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();

        var existingPriceUnits = await priceUnitRepository.Get(new PriceUnitsWithoutSpecification()).ToListAsync();

        if (existingPriceUnits.Any())
        {
            //foreach (var existingPriceUnit in existingPriceUnits)
            //{
            //    switch (existingPriceUnit.UnitType)
            //    {
            //        case UnitType.Gold18K when existingPriceUnit.Title == UnitType.Gold18K.GetDisplayName():
            //            existingPriceUnit.SetTitle("گرم");
            //            await priceUnitRepository.UpdateAsync(existingPriceUnit);
            //            break;
            //        case UnitType.Mesghal when existingPriceUnit.Title == UnitType.Mesghal.GetDisplayName():
            //            existingPriceUnit.SetTitle("مثقال");
            //            await priceUnitRepository.UpdateAsync(existingPriceUnit);
            //            break;
            //    }
            //}

            return;
        }

        var priceUnitsToCreate = Enum.GetValues<UnitType>()
            .Select(unitType => PriceUnit.Create(
                unitType.GetDisplayName(),
                unitType,
                unitType == UnitType.IRR))
            .ToList();

        await priceUnitRepository.CreateRangeAsync(priceUnitsToCreate);
    }

    private static async Task PopulateAdministratorClaimsAsync(IEnumerable<IApplicationPolicyProvider> policyProviders, IAccountService accountService)
    {
        var policies = policyProviders
            .SelectMany(x => x.GetPolicies())
            .SelectMany(x => x.Policies);

        foreach (var policy in policies)
        {
            var claimRequirements = policy.Requirements.OfType<ClaimsAuthorizationRequirement>();

            foreach (var claimRequirement in claimRequirements)
                if (claimRequirement.AllowedValues is null)
                    await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators,
                        new Claim(claimRequirement.ClaimType, string.Empty));
                else
                    foreach (var requiredValue in claimRequirement.AllowedValues)
                        await accountService.AddRoleClaimAsync(BuiltinRoles.Administrators, new Claim(claimRequirement.ClaimType, requiredValue));
        }
    }

    private static async Task PopulateDefaultSettingsAsync(IServiceProvider provider)
    {
        var serverSettingService = provider.GetRequiredService<IServerSettingService>();
        var settingsService = provider.GetRequiredService<ISettingService>();

        var settings = await settingsService.GetAsync();

        if (settings is null)
        {
            var defaultSettingOptions = provider.GetRequiredService<IOptions<DefaultSetting>>();
            var defaultSetting = defaultSettingOptions.Value;

            var setting = new CreateSettingRequest(defaultSetting.InstitutionName,
                defaultSetting.Address,
                defaultSetting.PhoneNumber,
                defaultSetting.TaxPercent,
                defaultSetting.GoldProfitPercent,
                defaultSetting.JewelryProfitPercent,
                defaultSetting.MoltenGoldCommissionPercent,
                defaultSetting.GoldSafetyMarginPercent,
                defaultSetting.UsedGoldFinenessDeductionRate,
                defaultSetting.GramPerMesghal,
                defaultSetting.PriceUpdateInterval);

            await serverSettingService.CreateAsync(setting);
        }
    }
}