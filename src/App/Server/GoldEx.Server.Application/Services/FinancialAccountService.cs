using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Application.Validators.FinancialAccounts;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class FinancialAccountService(
    IFinancialAccountRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    FinancialAccountRequestDtoValidator validator,
    DeleteFinancialAccountValidator deleteValidator,
    IMapper mapper) : IFinancialAccountService
{
    public async Task<List<GetFinancialAccountResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new FinancialAccountsDefaultSpecification(true))
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
            .Include(x => x.PriceUnit)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetFinancialAccountTitleResponse>>(list);
    }

    public async Task<GetFinancialAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetFinancialAccountResponse>(item);
    }

    public async Task CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        FinancialAccount financialAccount;

        if (request.CustomerId.HasValue)
        {
            financialAccount = FinancialAccount.CreateCustomerAccount(
                request.FinancialAccountType,
                new PriceUnitId(request.PriceUnitId),
                new CustomerId(request.CustomerId.Value));
        }
        else
        {
            if (!request.LedgerAccountId.HasValue)
                throw new ArgumentException("Ledger account ID is required for system accounts.", nameof(request.LedgerAccountId));

            var ledgerAccount = await ledgerAccountRepository
                .Get(new LedgerAccountsByIdSpecification(new LedgerAccountId(request.LedgerAccountId.Value)))
                .FirstOrDefaultAsync(cancellationToken)
                                ?? throw new InvalidOperationException($"System ledger account '{request.LedgerAccountId.Value}' not found.");

            financialAccount = FinancialAccount.CreateSystemAccount(
                request.FinancialAccountType,
                new PriceUnitId(request.PriceUnitId),
                ledgerAccount.Id);
        }

        switch (request.FinancialAccountType)
        {
            case FinancialAccountType.LocalBankAccount when request.LocalBankAccount is not null:
                financialAccount.SetLocalAccount(LocalBankAccount.Create(request.LocalBankAccount.AccountHolderName,
                    request.LocalBankAccount.BankName,
                    request.LocalBankAccount.CardNumber,
                    request.LocalBankAccount.ShabaNumber,
                    request.LocalBankAccount.AccountNumber));
                break;
            case FinancialAccountType.InternationalBankAccount when request.InternationalBankAccount is not null:
                financialAccount.SetInternationalAccount(InternationalBankAccount.Create(request.InternationalBankAccount.AccountHolderName,
                    request.InternationalBankAccount.BankName,
                    request.InternationalBankAccount.SwiftBicCode,
                    request.InternationalBankAccount.IbanNumber,
                    request.InternationalBankAccount.AccountNumber));
                break;
            case FinancialAccountType.Cash:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await repository.CreateAsync(financialAccount, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var financialAccount = await repository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        financialAccount.SetAccountType(request.FinancialAccountType);
        financialAccount.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));

        if (financialAccount.IsSystemAccount)
        {
            if (!request.LedgerAccountId.HasValue)
                throw new ArgumentException("Ledger account ID is required for system accounts.", nameof(request.LedgerAccountId));

            var ledgerAccount = await ledgerAccountRepository
                                    .Get(new LedgerAccountsByIdSpecification(new LedgerAccountId(request.LedgerAccountId.Value)))
                                    .FirstOrDefaultAsync(cancellationToken)
                                ?? throw new InvalidOperationException($"System ledger account '{request.LedgerAccountId.Value}' not found.");

            financialAccount.SetLedgerAccount(ledgerAccount.Id);
        }
        else
            financialAccount.SetLedgerAccount(null);

        switch (request.FinancialAccountType)
        {
            case FinancialAccountType.LocalBankAccount when request.LocalBankAccount is not null:
                financialAccount.SetLocalAccount(LocalBankAccount.Create(request.LocalBankAccount.AccountHolderName,
                    request.LocalBankAccount.BankName,
                    request.LocalBankAccount.CardNumber,
                    request.LocalBankAccount.ShabaNumber,
                    request.LocalBankAccount.AccountNumber));
                break;
            case FinancialAccountType.InternationalBankAccount when request.InternationalBankAccount is not null:
                financialAccount.SetInternationalAccount(InternationalBankAccount.Create(request.InternationalBankAccount.AccountHolderName,
                    request.InternationalBankAccount.BankName,
                    request.InternationalBankAccount.SwiftBicCode,
                    request.InternationalBankAccount.IbanNumber,
                    request.InternationalBankAccount.AccountNumber));
                break;
            case FinancialAccountType.Cash:
                financialAccount.SetCashAccount();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await repository.UpdateAsync(financialAccount, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var financialAccount = await repository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(financialAccount, cancellationToken);

        await repository.DeleteAsync(financialAccount, cancellationToken);
    }
}