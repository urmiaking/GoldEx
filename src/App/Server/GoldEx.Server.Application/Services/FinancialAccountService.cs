using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.FinancialAccounts;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class FinancialAccountService(
    IFinancialAccountRepository repository,
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

        var financialAccount = FinancialAccount.CreateSystemAccount(request.FinancialAccountType, new PriceUnitId(request.PriceUnitId));

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