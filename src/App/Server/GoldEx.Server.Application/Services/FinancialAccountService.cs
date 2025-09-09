using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.FinancialAccounts;
using GoldEx.Server.Application.Validators.LedgerAccounts;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class FinancialAccountService(
    IFinancialAccountRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    IPriceUnitRepository priceUnitRepository,
    FinancialAccountRequestDtoValidator validator,
    DeleteFinancialAccountValidator deleteValidator,
    DeleteLedgerAccountValidator deleteLedgerAccountValidator,
    IMapper mapper,
    ILogger<FinancialAccountService> logger) : IFinancialAccountService
{
    public async Task<List<GetFinancialAccountResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new FinancialAccountsDefaultSpecification(true))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetFinancialAccountResponse>>(list);
    }

    public async Task<List<GetFinancialAccountTitleResponse>> GetTitlesAsync(Guid? customerId, Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        ISpecification<FinancialAccount> specification = customerId.HasValue
            ? new FinancialAccountsByCustomerIdSpecification(new CustomerId(customerId.Value),
                priceUnitId.HasValue ? new PriceUnitId(priceUnitId.Value) : null)
            : new FinancialAccountsDefaultSpecification(true, priceUnitId.HasValue ? new PriceUnitId(priceUnitId.Value) : null);

        var list = await repository
            .Get(specification)
            .AsNoTracking()
            .Include(x => x.PriceUnit)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetFinancialAccountTitleResponse>>(list);
    }

    public async Task<GetFinancialAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetFinancialAccountResponse>(item);
    }

    public async Task CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                FinancialAccount financialAccount;

                if (request is { CustomerId: not null, IsSystemAccount: false })
                {
                    financialAccount = FinancialAccount.CreateCustomerAccount(
                        request.HolderName,
                        request.BrokerName,
                        request.FinancialAccountType,
                        new PriceUnitId(request.PriceUnitId),
                        new CustomerId(request.CustomerId.Value));
                }
                else
                {
                    var ledgerAccountId = await GetOrCreateSystemLedgerAccountAsync(request, cancellationToken);

                    financialAccount = FinancialAccount.CreateSystemAccount(
                        request.HolderName,
                        request.BrokerName,
                        request.FinancialAccountType,
                        new PriceUnitId(request.PriceUnitId),
                        ledgerAccountId);
                }

                switch (request.FinancialAccountType)
                {
                    case FinancialAccountType.LocalBankAccount when request.LocalBankAccount is not null:
                        financialAccount.SetLocalAccount(LocalBankAccount.Create(
                            request.LocalBankAccount.CardNumber,
                            request.LocalBankAccount.ShabaNumber,
                            request.LocalBankAccount.AccountNumber));
                        break;
                    case FinancialAccountType.InternationalBankAccount when request.InternationalBankAccount is not null:
                        financialAccount.SetInternationalAccount(InternationalBankAccount.Create(
                            request.InternationalBankAccount.SwiftBicCode,
                            request.InternationalBankAccount.IbanNumber,
                            request.InternationalBankAccount.AccountNumber));
                        break;
                    case FinancialAccountType.Cash when request.CashAccount is not null:
                        financialAccount.SetCashAccount(CashAccount.Create(request.CashAccount.Title, request.CashAccount.AccountType));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await repository.CreateAsync(financialAccount, cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var financialAccount = await repository
                    .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
                    .Include(x => x.PriceUnit)
                    .Include(x => x.LedgerAccount)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                financialAccount.SetAccountType(request.FinancialAccountType);
                financialAccount.SetHolderName(request.HolderName);
                financialAccount.SetBrokerName(request.BrokerName);
                financialAccount.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));

                switch (request.FinancialAccountType)
                {
                    case FinancialAccountType.LocalBankAccount when request.LocalBankAccount is not null:
                        financialAccount.SetLocalAccount(LocalBankAccount.Create(
                            request.LocalBankAccount.CardNumber,
                            request.LocalBankAccount.ShabaNumber,
                            request.LocalBankAccount.AccountNumber));
                        break;
                    case FinancialAccountType.InternationalBankAccount when request.InternationalBankAccount is not null:
                        financialAccount.SetInternationalAccount(InternationalBankAccount.Create(
                            request.InternationalBankAccount.SwiftBicCode,
                            request.InternationalBankAccount.IbanNumber,
                            request.InternationalBankAccount.AccountNumber));
                        break;
                    case FinancialAccountType.Cash when request.CashAccount is not null:
                        financialAccount.SetCashAccount(CashAccount.Create(request.CashAccount.Title, request.CashAccount.AccountType));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (financialAccount.IsSystemAccount)
                {
                    var ledgerAccount = financialAccount.LedgerAccount;

                    if (ledgerAccount is null)
                        throw new InvalidOperationException("System financial accounts must have a ledger account.");

                    var priceUnit = await priceUnitRepository
                        .Get(new PriceUnitsByIdSpecification(new PriceUnitId(request.PriceUnitId)))
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Price unit with ID '{request.PriceUnitId}' not found.");

                    switch (request.FinancialAccountType)
                    {
                        case FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount:

                            var banksLedgerAccount = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Banks))
                                .FirstOrDefaultAsync(cancellationToken) 
                                                     ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.Banks)}' not found.");

                            ledgerAccount.SetTitle(LedgerAccountTitleBuilder.ForFinancialAccount(
                                request.FinancialAccountType,
                                request.BrokerName,
                                request.LocalBankAccount?.AccountNumber ?? request.InternationalBankAccount?.AccountNumber, null));

                            ledgerAccount.SetParentAccount(banksLedgerAccount.Id);

                            break;
                        case FinancialAccountType.Cash when financialAccount.CashAccount?.AccountType is CashAccountType.Internal:
                            var cashLedgerAccount = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.InternalCashAccounts))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.InternalCashAccounts)}' not found.");

                            ledgerAccount.SetTitle(LedgerAccountTitleBuilder.ForFinancialAccount(request.FinancialAccountType, null, null, priceUnit.Title));
                            ledgerAccount.SetParentAccount(cashLedgerAccount.Id);

                            break;
                        case FinancialAccountType.Cash when financialAccount.CashAccount?.AccountType is CashAccountType.DepositsWithOthers:
                            var depositsLedgerAccount = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.DepositsWithOthers))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.DepositsWithOthers)}' not found.");
                            
                            ledgerAccount.SetTitle(LedgerAccountTitleBuilder.ForFinancialAccount(request.FinancialAccountType, request.BrokerName, null, priceUnit.Title));
                            ledgerAccount.SetParentAccount(depositsLedgerAccount.Id);

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    // TODO: need to be reviewed
                    financialAccount.SetLedgerAccount(ledgerAccount.Id);

                    await ledgerAccountRepository.UpdateAsync(ledgerAccount, cancellationToken);
                }
                else
                    financialAccount.SetLedgerAccount(null);

                await repository.UpdateAsync(financialAccount, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var financialAccount = await repository
                    .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
                    .Include(x => x.PriceUnit)
                    .Include(x => x.LedgerAccount)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                await deleteValidator.ValidateAndThrowAsync(financialAccount, cancellationToken);
                await repository.DeleteAsync(financialAccount, cancellationToken);

                if (financialAccount is { IsSystemAccount: true, AccountType: not FinancialAccountType.Cash, LedgerAccount: not null })
                {
                    await deleteLedgerAccountValidator.ValidateAndThrowAsync(financialAccount.LedgerAccount, cancellationToken);
                    await ledgerAccountRepository.DeleteAsync(financialAccount.LedgerAccount, cancellationToken);
                }

                if (financialAccount is
                    {
                        AccountType: FinancialAccountType.Cash,
                        CashAccount.AccountType: CashAccountType.DepositsWithOthers,
                        LedgerAccount: not null
                    })
                {
                    await deleteLedgerAccountValidator.ValidateAndThrowAsync(financialAccount.LedgerAccount, cancellationToken);
                    await ledgerAccountRepository.DeleteAsync(financialAccount.LedgerAccount, cancellationToken);
                }

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    private async Task<LedgerAccountId> GetOrCreateSystemLedgerAccountAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken)
    {
        LedgerAccount parentLedgerAccount;
        string ledgerAccountTitle;

        var priceUnit = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(request.PriceUnitId)))
            .FirstOrDefaultAsync(cancellationToken) 
                        ?? throw new NotFoundException($"Price unit with ID '{request.PriceUnitId}' not found.");

        switch (request.FinancialAccountType)
        {
            case FinancialAccountType.LocalBankAccount or FinancialAccountType.InternationalBankAccount:
                parentLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Banks))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.Banks)}' not found.");
                ledgerAccountTitle = LedgerAccountTitleBuilder.ForFinancialAccount(
                    request.FinancialAccountType,
                    request.BrokerName,
                    request.LocalBankAccount?.AccountNumber ?? request.InternationalBankAccount?.AccountNumber, null);
                break;
            case FinancialAccountType.Cash when request.CashAccount?.AccountType is CashAccountType.Internal:
                parentLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CashAccounts))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.CashAccounts)}' not found.");

                ledgerAccountTitle = LedgerAccountTitleBuilder.ForFinancialAccount(request.FinancialAccountType, null, null, priceUnit.Title);
                break;
            case FinancialAccountType.Cash when request.CashAccount?.AccountType is CashAccountType.DepositsWithOthers:

                parentLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.DepositsWithOthers))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"System ledger account '{nameof(SystemLedgerAccounts.DepositsWithOthers)}' not found.");

                ledgerAccountTitle = LedgerAccountTitleBuilder.ForFinancialAccount(request.FinancialAccountType, request.BrokerName, null, priceUnit.Title);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var existingAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(ledgerAccountTitle))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAccount is not null)
            return existingAccount.Id;

        var newLedgerAccount = LedgerAccount.CreateSystemAccount(ledgerAccountTitle, LedgerAccountType.Asset, parentLedgerAccount.Id);
        await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
        return newLedgerAccount.Id;
    }
}