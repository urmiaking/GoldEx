using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.Constants;

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
        await PopulateDefaultProductCategoriesAsync(serviceProvider);
        await PopulateDefaultLedgerAccountsAsync(serviceProvider);

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

    private static async Task PopulateDefaultLedgerAccountsAsync(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<ILedgerAccountRepository>();

        if (await repository.ExistsAsync(new LedgerAccountsByTypeSpecification(true)))
            return;

        var assets = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Assets, LedgerAccountType.Asset);
        var liabilities = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Liabilities, LedgerAccountType.Liability);
        var equity = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Equity, LedgerAccountType.Equity);
        var revenue = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Revenue, LedgerAccountType.Revenue);
        var expenses = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Expenses, LedgerAccountType.Expense);

        var topLevelAccounts = new List<LedgerAccount> { assets, liabilities, equity, revenue, expenses };
        await repository.CreateRangeAsync(topLevelAccounts);

        var currentAssets = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.CurrentAssets, LedgerAccountType.Asset);
        currentAssets.SetParentAccount(assets.Id);

        var accountsReceivable = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.AccountsReceivable, LedgerAccountType.Asset);
        accountsReceivable.SetParentAccount(currentAssets.Id);

        var prepayments = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.PrepaymentsToSuppliers, LedgerAccountType.Asset);
        prepayments.SetParentAccount(currentAssets.Id);

        var inventory = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Inventory, LedgerAccountType.Asset);
        inventory.SetParentAccount(currentAssets.Id);

        var bankAccounts = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.Banks, LedgerAccountType.Asset);
        bankAccounts.SetParentAccount(currentAssets.Id);

        var cashAccounts = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.CashAccounts, LedgerAccountType.Asset);
        cashAccounts.SetParentAccount(currentAssets.Id);

        var currentLiabilities = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.CurrentLiabilities, LedgerAccountType.Liability);
        currentLiabilities.SetParentAccount(liabilities.Id);

        var accountsPayable = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.AccountsPayable, LedgerAccountType.Liability);
        accountsPayable.SetParentAccount(currentLiabilities.Id);

        var salesRevenue = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.SalesRevenue, LedgerAccountType.Revenue);
        salesRevenue.SetParentAccount(revenue.Id);

        var additionalChargesRevenue = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.AdditionalChargesRevenue, LedgerAccountType.Revenue);
        additionalChargesRevenue.SetParentAccount(revenue.Id);

        var exchangeGainLoss = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.ExchangeGainLoss, LedgerAccountType.Revenue);
        exchangeGainLoss.SetParentAccount(revenue.Id);

        var cogs = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.CostOfGoodsSold, LedgerAccountType.Expense);
        cogs.SetParentAccount(expenses.Id);

        var operatingExpenses = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.OperatingExpenses, LedgerAccountType.Expense);
        operatingExpenses.SetParentAccount(expenses.Id);

        var salesDiscounts = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.SalesDiscounts, LedgerAccountType.Expense);
        salesDiscounts.SetParentAccount(expenses.Id);

        var purchaseDiscounts = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.PurchaseDiscounts, LedgerAccountType.Expense);
        purchaseDiscounts.SetParentAccount(expenses.Id);

        var subLevelAccounts = new List<LedgerAccount>
        {
            currentAssets, accountsReceivable, prepayments, inventory, bankAccounts, cashAccounts,
            currentLiabilities, accountsPayable,
            salesRevenue, additionalChargesRevenue, exchangeGainLoss,
            cogs, operatingExpenses, salesDiscounts, purchaseDiscounts
        };
        await repository.CreateRangeAsync(subLevelAccounts);
    }

    private static async Task PopulateDefaultProductCategoriesAsync(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IProductCategoryRepository>();

        var productCategoryCount = await repository.CountAsync(new ProductCategoriesDefaultSpecification());

        if (productCategoryCount > 0)
            return;

        var defaultCategories = new List<ProductCategory>
        {
            ProductCategory.Create("انگشتر"),
            ProductCategory.Create("النگو"),
            ProductCategory.Create("دستبند"),
            ProductCategory.Create("گوشواره"),
            ProductCategory.Create("گردنبند"),
        };

        await repository.CreateRangeAsync(defaultCategories);
    }

    private static async Task PopulateDefaultPriceUnitsAsync(IServiceProvider serviceProvider)
    {
        var priceUnitRepository = serviceProvider.GetRequiredService<IPriceUnitRepository>();

        if (!await priceUnitRepository.ExistsAsync(new PriceUnitsWithoutSpecification()))
        {
            var unitTypes = Enum.GetValues<UnitType>()
                .Select(x => PriceUnit.Create(x.GetDisplayName(), x == UnitType.IRR))
                .ToList();

            await priceUnitRepository.CreateRangeAsync(unitTypes);
            return;
        }

        if (await priceUnitRepository.ExistsAsync(new PriceUnitsSetAsDefaultSpecification()))
            return;

        var defaultUnit = await priceUnitRepository
            .Get(new PriceUnitsByTitleSpecification(UnitType.IRR.GetDisplayName()))
            .FirstOrDefaultAsync();

        if (defaultUnit is not null)
        {
            defaultUnit.SetDefault(true);
            defaultUnit.SetStatus(true);
            await priceUnitRepository.UpdateAsync(defaultUnit);
        }
        else
        {
            defaultUnit = PriceUnit.Create(UnitType.IRR.GetDisplayName(), true);
            await priceUnitRepository.CreateAsync(defaultUnit);
        }
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
                defaultSetting.GoldSafetyMarginPercent,
                defaultSetting.OldGoldCarat,
                defaultSetting.PriceUpdateInterval);

            await serverSettingService.CreateAsync(setting);
        }
    }
}