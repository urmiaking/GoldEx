using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.LedgerAccounts;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class LedgerAccountService(
    ILedgerAccountRepository repository,
    IMapper mapper,
    LedgerAccountRequestDtoValidator validator,
    DeleteLedgerAccountValidator deleteValidator) : ILedgerAccountService
{
    public async Task<List<GetLedgerAccountResponse>> GetListAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        var spec = new LedgerAccountsByCustomerIdSpecification(customerId.HasValue ? new CustomerId(customerId.Value) : null);
        var list = await repository.Get(spec).AsNoTracking().ToListAsync(cancellationToken);

        return mapper.Map<List<GetLedgerAccountResponse>>(list);
    }

    public async Task<List<GetLedgerAccountResponse>> GetTitlesAsync(FinancialAccountType? financialAccountType, CancellationToken cancellationToken = default)
    {
        var spec = new LedgerAccountsByFinancialAccountTypeSpecification(financialAccountType);
        var list = await repository.Get(spec).AsNoTracking().ToListAsync(cancellationToken);

        return mapper.Map<List<GetLedgerAccountResponse>>(list);
    }

    public async Task<GetLedgerAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new LedgerAccountsByIdSpecification(new LedgerAccountId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetLedgerAccountResponse>(item);
    }

    public async Task CreateAsync(LedgerAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var ledgerAccount = request.IsSystemAccount
            ? LedgerAccount.CreateSystemAccount(request.Title, request.AccountType, request.ParentAccountId.HasValue
                ? new LedgerAccountId(request.ParentAccountId.Value)
                : null)
            :
            LedgerAccount.CreateCustomerAccount(request.Title,
                new CustomerId(request.CustomerId!.Value),
                request.AccountType,
                request.ParentAccountId.HasValue
                    ? new LedgerAccountId(request.ParentAccountId.Value)
                    : null);

        await repository.CreateAsync(ledgerAccount, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, LedgerAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var ledgerAccount = await repository
            .Get(new LedgerAccountsByIdSpecification(new LedgerAccountId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        ledgerAccount.SetTitle(request.Title);

        await repository.UpdateAsync(ledgerAccount, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ledgerAccount = await repository
            .Get(new LedgerAccountsByIdSpecification(new LedgerAccountId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(ledgerAccount, cancellationToken);

        await repository.DeleteAsync(ledgerAccount, cancellationToken);
    }
}