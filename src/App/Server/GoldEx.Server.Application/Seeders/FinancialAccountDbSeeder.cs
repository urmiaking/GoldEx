using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class FinancialAccountDbSeeder(
    IFinancialAccountRepository financialAccountRepository,
    IPriceUnitRepository priceUnitRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    ILogger<FinancialAccountDbSeeder> logger) : IDbSeeder
{
    public int Order => 40;
    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        await EnsureGoldAccountExistsAsync(cancellationToken);
        await EnsureCashAccountExistsAsync(cancellationToken);
    }

    private async Task EnsureGoldAccountExistsAsync(CancellationToken cancellationToken = default)
    {
        if (await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Gold), cancellationToken))
            return;

        var goldPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
            .FirstOrDefaultAsync(cancellationToken) 
                            ?? throw new NotFoundException("Gold price unit is not initialized");

        var inventoryLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
            .FirstOrDefaultAsync(cancellationToken) 
                                     ?? throw new NotFoundException("Molten Gold Inventory ledger account is not initialized");

        var goldAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Gold, goldPriceUnit.Id, inventoryLedgerAccount.Id);
        await financialAccountRepository.CreateAsync(goldAccount, cancellationToken);

        logger.LogInformation($"{nameof(FinancialAccountDbSeeder)}: Seeded gold financial account.");
    }

    private async Task EnsureCashAccountExistsAsync(CancellationToken cancellationToken = default)
    {
        if (await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Cash), cancellationToken))
            return;

        var tomanPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByTitleSpecification(UnitType.TMN.GetDisplayName()))
            .FirstOrDefaultAsync(cancellationToken) 
                             ?? throw new NotFoundException("Toman price unit is not initialized");

        var internalCashLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.InternalCashAccounts))
            .FirstOrDefaultAsync(cancellationToken) 
                                        ?? throw new NotFoundException("Internal Cash Accounts ledger account is not initialized");

        var cashAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Cash, tomanPriceUnit.Id, internalCashLedgerAccount.Id,
            cashAccount: CashAccount.Create(null, CashAccountType.Internal));

        await financialAccountRepository.CreateAsync(cashAccount, cancellationToken);

        logger.LogInformation($"{nameof(FinancialAccountDbSeeder)}: Seeded cash financial account.");
    }
}