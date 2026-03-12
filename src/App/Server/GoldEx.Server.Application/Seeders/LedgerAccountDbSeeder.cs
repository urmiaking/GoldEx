using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class LedgerAccountDbSeeder(
    ILedgerAccountRepository ledgerAccountRepository,
    IPriceUnitRepository priceUnitRepository,
    ILogger<LedgerAccountDbSeeder> logger) : IDbSeeder
{
    public int Order => 30;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
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
        await GetOrCreateAccount(SystemLedgerAccounts.OpeningBalanceEquity, LedgerAccountType.Equity, equity);

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
        await GetOrCreateAccount(SystemLedgerAccounts.CurrencySettlement, LedgerAccountType.Asset, currentAssets);

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
        await GetOrCreateAccount(SystemLedgerAccounts.ShortageExpense, LedgerAccountType.Expense, operatingExpenses);
        await GetOrCreateAccount(SystemLedgerAccounts.TheftLoss, LedgerAccountType.Expense, operatingExpenses);
        await GetOrCreateAccount(SystemLedgerAccounts.DamageExpense, LedgerAccountType.Expense, operatingExpenses);
        await GetOrCreateAccount(SystemLedgerAccounts.GiftExpense, LedgerAccountType.Expense, operatingExpenses);

        await GetOrCreateAccount(SystemLedgerAccounts.OwnerDraw, LedgerAccountType.Equity, equity);
    }

    private async Task<LedgerAccount> GetOrCreateAccount(string title, LedgerAccountType type, LedgerAccount? parent = null, UnitType? unitType = null)
    {
        var existingAccount = await ledgerAccountRepository
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

                await ledgerAccountRepository.UpdateAsync(existingAccount);
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
        await ledgerAccountRepository.CreateAsync(newAccount);

        logger.LogInformation($"{nameof(LedgerAccountDbSeeder)}: Seeded '{newAccount.AccountType.ToString()}' ledger account.");

        return newAccount;
    }
}