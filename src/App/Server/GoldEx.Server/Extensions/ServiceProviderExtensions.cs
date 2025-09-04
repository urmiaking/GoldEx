using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Authorization;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
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
using GoldEx.Shared.DTOs.PriceUnits;

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
        await PopulateDefaultLedgerAccountsAsync(serviceProvider);
        await PopulateDefaultPriceUnitsAsync(serviceProvider);
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

    private static async Task PopulateDefaultLedgerAccountsAsync(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<ILedgerAccountRepository>();

        // متد کمکی برای جلوگیری از تکرار کد
        async Task<LedgerAccount> GetOrCreateAccount(string title, LedgerAccountType type, LedgerAccount? parent = null)
        {
            var existingAccount = await repository
                .Get(new LedgerAccountsByTitleSpecification(title))
                .FirstOrDefaultAsync();

            if (existingAccount is not null)
            {
                return existingAccount;
            }

            var newAccount = LedgerAccount.CreateSystemAccount(title, type, parent?.Id);
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
        await GetOrCreateAccount(SystemLedgerAccounts.UsedProductInventory, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.CoinInventory, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.Banks, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.DepositsWithOthers, LedgerAccountType.Asset, currentAssets);

        var cashAccounts = await GetOrCreateAccount(SystemLedgerAccounts.CashAccounts, LedgerAccountType.Asset, currentAssets);
        await GetOrCreateAccount(SystemLedgerAccounts.InternalCashAccounts, LedgerAccountType.Asset, cashAccounts);

        await GetOrCreateAccount(SystemLedgerAccounts.AccountsPayable, LedgerAccountType.Liability, currentLiabilities);

        await GetOrCreateAccount(SystemLedgerAccounts.SalesRevenue, LedgerAccountType.Revenue, revenue);
        await GetOrCreateAccount(SystemLedgerAccounts.AdditionalChargesRevenue, LedgerAccountType.Revenue, revenue);
        await GetOrCreateAccount(SystemLedgerAccounts.ExchangeGainLoss, LedgerAccountType.Revenue, revenue);

        await GetOrCreateAccount(SystemLedgerAccounts.CostOfGoodsSold, LedgerAccountType.Expense, expenses);
        await GetOrCreateAccount(SystemLedgerAccounts.SalesDiscounts, LedgerAccountType.Expense, expenses);
        await GetOrCreateAccount(SystemLedgerAccounts.PurchaseDiscounts, LedgerAccountType.Expense, expenses);
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
        var priceUnitService = serviceProvider.GetRequiredService<IPriceUnitService>();

        var priceUnits = await priceUnitService.GetAllAsync();

        if (!priceUnits.Any())
            foreach (var unitType in Enum.GetValues<UnitType>()) 
                await priceUnitService.CreateAsync(new CreatePriceUnitRequest(unitType.GetDisplayName(), null, null));

        var defaultPriceUnit = priceUnits.FirstOrDefault(x => x.IsDefault);

        if (defaultPriceUnit is null)
        {
            var irrUnit = priceUnits.FirstOrDefault(x => x.Title == UnitType.IRR.GetDisplayName());

            if (irrUnit is not null) 
                await priceUnitService.SetAsDefaultAsync(irrUnit.Id);
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
                defaultSetting.MoltenGoldCommissionPercent,
                defaultSetting.GoldSafetyMarginPercent,
                defaultSetting.OldGoldCarat,
                defaultSetting.PriceUpdateInterval);

            await serverSettingService.CreateAsync(setting);
        }
    }
}