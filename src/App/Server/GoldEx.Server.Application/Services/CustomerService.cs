using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CustomerService(
    ICustomerRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    IFinancialAccountRepository financialAccountRepository,
    IMapper mapper,
    ILogger<CustomerService> logger,
    CustomerRequestDtoValidator validator,
    DeleteCustomerValidator deleteValidator) : ICustomerService
{
    public async Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CustomerFilter customerFilter,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await repository
            .Get(new CustomersByFilterSpecification(filter, customerFilter))
            .AsNoTracking()
            .Include(x => x.FinancialAccounts!)
                .ThenInclude(x => x.PriceUnit)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(new CustomersByFilterSpecification(filter, customerFilter), cancellationToken);

        return new PagedList<GetCustomerResponse>
        {
            Data = mapper.Map<List<GetCustomerResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<List<GetCustomerResponse>> GetByNameAsync(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerName))
            return [];

        var items = await repository
            .Get(new CustomersByNameSpecification(customerName))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetCustomerResponse>>(items);
    }

    public async Task<GetCustomerResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCustomerResponse>(item);
    }

    public async Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByNationalIdSpecification(nationalId))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null ? mapper.Map<GetCustomerResponse>(item) : null;
    }

    public async Task<GetCustomerResponse?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByPhoneNumberSpecification(phoneNumber))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null ? mapper.Map<GetCustomerResponse>(item) : null;
    }

    public async Task<Guid> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var customer = Customer.Create(request.CustomerType,
                request.FullName,
                request.NationalId,
                request.PhoneNumber,
                request.Address,
                request.CreditLimit,
                request.CreditLimitPriceUnitId.HasValue ? new PriceUnitId(request.CreditLimitPriceUnitId.Value) : null);

                await repository.CreateAsync(customer, cancellationToken);

                if (request.FinancialAccounts is not null && request.FinancialAccounts.Any())
                {
                    var financialAccounts = new List<FinancialAccount>();
                    foreach (var dto in request.FinancialAccounts)
                    {
                        var (localAccount, internationalAccount, cashAccount) = CreateAccountDetailsFromDto(dto);

                        var bankAccount = FinancialAccount.CreateCustomerAccount(
                            dto.HolderName,
                            dto.BrokerName,
                            dto.FinancialAccountType,
                            new PriceUnitId(dto.PriceUnitId),
                            customer.Id,
                            localAccount,
                            internationalAccount,
                            cashAccount);

                        financialAccounts.Add(bankAccount);
                    }
                    await financialAccountRepository.CreateRangeAsync(financialAccounts, cancellationToken);
                }

                await dbTransaction.CommitAsync(cancellationToken);

                return customer.Id.Value;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var customer = await repository
                    .Get(new CustomersByIdSpecification(new CustomerId(id)))
                    .Include(x => x.FinancialAccounts)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                var customerOldName = customer.FullName;

                customer.SetFullName(request.FullName);
                customer.SetNationalId(request.NationalId);
                customer.SetPhoneNumber(request.PhoneNumber);
                customer.SetAddress(request.Address);
                customer.SetCustomerType(request.CustomerType);
                customer.SetCreditLimit(request.CreditLimit,
                    request.CreditLimitPriceUnitId.HasValue
                        ? new PriceUnitId(request.CreditLimitPriceUnitId.Value)
                        : null);

                if (request.FinancialAccounts is not null) 
                    await SyncFinancialAccounts(customer, request.FinancialAccounts, cancellationToken);

                await SyncLedgerAccountTitles(customer, customerOldName, cancellationToken);

                await repository.UpdateAsync(customer, cancellationToken);

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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var item = await repository
                    .Get(new CustomersByIdSpecification(new CustomerId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

                var financialAccounts = await financialAccountRepository
                    .Get(new FinancialAccountsByCustomerIdSpecification(item.Id))
                    .ToListAsync(cancellationToken);

                if (financialAccounts.Any())
                {
                    await financialAccountRepository.DeleteRangeAsync(financialAccounts, cancellationToken);
                }

                var ledgerAccounts = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerIdSpecification(item.Id))
                    .ToListAsync(cancellationToken);

                if (ledgerAccounts.Any())
                    await ledgerAccountRepository.DeleteRangeAsync(ledgerAccounts, cancellationToken);

                await repository.DeleteAsync(item, cancellationToken);

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

    /// <summary>
    /// Synchronizes the customer's financial accounts based on the request.
    /// </summary>
    private async Task SyncFinancialAccounts(Customer customer, List<FinancialAccountRequestDto> accountRequests, CancellationToken cancellationToken)
    {
        var existingAccounts = customer.FinancialAccounts?.ToList() ?? [];
        var accountsToRemove = existingAccounts.Where(e => accountRequests.All(r => r.Id != e.Id.Value)).ToList();

        if (accountsToRemove.Any()) 
            await financialAccountRepository.DeleteRangeAsync(accountsToRemove, cancellationToken);

        foreach (var request in accountRequests)
        {
            var existingAccount = existingAccounts.FirstOrDefault(e => e.Id.Value == request.Id);
            var (localAccount, internationalAccount, cashAccount) = CreateAccountDetailsFromDto(request);

            if (existingAccount is not null)
            {
                existingAccount.Update(
                    request.HolderName,
                    request.BrokerName,
                    request.FinancialAccountType,
                    new PriceUnitId(request.PriceUnitId),
                    localAccount,
                    internationalAccount,
                    cashAccount);

                await financialAccountRepository.UpdateAsync(existingAccount, cancellationToken);
            }
            else
            {
                var newAccount = FinancialAccount.CreateCustomerAccount(
                    request.HolderName,
                    request.BrokerName,
                    request.FinancialAccountType,
                    new PriceUnitId(request.PriceUnitId),
                    customer.Id,
                    localAccount,
                    internationalAccount,
                    cashAccount);

                await financialAccountRepository.CreateAsync(newAccount, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Creates the appropriate account detail value object based on the DTO.
    /// </summary>
    private (LocalBankAccount?, InternationalBankAccount?, CashAccount?) CreateAccountDetailsFromDto(FinancialAccountRequestDto request)
    {
        switch (request.FinancialAccountType)
        {
            case FinancialAccountType.LocalBankAccount when request.LocalBankAccount is not null:
                return (LocalBankAccount.Create(
                    request.LocalBankAccount.CardNumber,
                    request.LocalBankAccount.ShabaNumber,
                    request.LocalBankAccount.AccountNumber), null, null);

            case FinancialAccountType.InternationalBankAccount when request.InternationalBankAccount is not null:
                return (null, InternationalBankAccount.Create(
                    request.InternationalBankAccount.SwiftBicCode,
                    request.InternationalBankAccount.IbanNumber,
                    request.InternationalBankAccount.AccountNumber), null);

            case FinancialAccountType.Cash when request.CashAccount is not null:
                return (null, null, CashAccount.Create(
                    request.CashAccount.Title,
                    request.CashAccount.AccountType));

            default:
                return (null, null, null);
        }
    }

    // Placeholder for your existing ledger sync logic
    private async Task SyncLedgerAccountTitles(Customer customer, string oldName, CancellationToken cancellationToken)
    {
        var ledgerAccounts = await ledgerAccountRepository
            .Get(new LedgerAccountsByCustomerIdSpecification(customer.Id))
            .ToListAsync(cancellationToken);

        if (ledgerAccounts.Any())
        {
            foreach (var ledgerAccount in ledgerAccounts)
            {
                ledgerAccount.SetTitle(ledgerAccount.Title.Replace(oldName, customer.FullName,
                    StringComparison.OrdinalIgnoreCase));
                await ledgerAccountRepository.UpdateAsync(ledgerAccount, cancellationToken);
            }
        }
    }
}