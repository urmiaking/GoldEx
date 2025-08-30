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

                List<FinancialAccount> bankAccounts = [];

                if (request.FinancialAccounts is not null)
                {
                    foreach (var financialAccountRequestDto in request.FinancialAccounts)
                    {
                        var bankAccount = FinancialAccount.CreateCustomerAccount(financialAccountRequestDto.FinancialAccountType,
                            new PriceUnitId(financialAccountRequestDto.PriceUnitId),
                            customer.Id);

                        switch (bankAccount.AccountType)
                        {
                            case FinancialAccountType.InternationalBankAccount when financialAccountRequestDto.InternationalBankAccount is not null:
                                bankAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                    financialAccountRequestDto.InternationalBankAccount.AccountHolderName,
                                    financialAccountRequestDto.InternationalBankAccount.BankName,
                                    financialAccountRequestDto.InternationalBankAccount.SwiftBicCode,
                                    financialAccountRequestDto.InternationalBankAccount.IbanNumber,
                                    financialAccountRequestDto.InternationalBankAccount.AccountNumber));
                                break;
                            case FinancialAccountType.LocalBankAccount when financialAccountRequestDto.LocalBankAccount is not null:
                                bankAccount.SetLocalAccount(LocalBankAccount.Create(
                                    financialAccountRequestDto.LocalBankAccount.AccountHolderName,
                                    financialAccountRequestDto.LocalBankAccount.BankName,
                                    financialAccountRequestDto.LocalBankAccount.CardNumber,
                                    financialAccountRequestDto.LocalBankAccount.ShabaNumber,
                                    financialAccountRequestDto.LocalBankAccount.AccountNumber));
                                break;
                            case FinancialAccountType.Cash:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        bankAccounts.Add(bankAccount);
                    }

                    await financialAccountRepository.CreateRangeAsync(bankAccounts, cancellationToken);
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
                {
                    var existingFinancialAccounts = customer.FinancialAccounts?.ToList() ?? [];
                    // Remove financial accounts that are not in the request
                    foreach (var existingAccount in existingFinancialAccounts)
                    {
                        if (request.FinancialAccounts.All(x => x.Id != existingAccount.Id.Value))
                        {
                            await financialAccountRepository.DeleteAsync(existingAccount, cancellationToken);
                        }
                    }

                    // Update or add new financial accounts
                    foreach (var bankAccountRequest in request.FinancialAccounts)
                    {
                        var existingAccount =
                            existingFinancialAccounts.FirstOrDefault(x => x.Id.Value == bankAccountRequest.Id);
                        if (existingAccount is not null)
                        {
                            existingAccount.SetAccountType(bankAccountRequest.FinancialAccountType);
                            existingAccount.SetPriceUnitId(new PriceUnitId(bankAccountRequest.PriceUnitId));

                            switch (existingAccount.AccountType)
                            {
                                case FinancialAccountType.InternationalBankAccount when bankAccountRequest.InternationalBankAccount is not null:
                                    existingAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                        bankAccountRequest.InternationalBankAccount.AccountHolderName,
                                        bankAccountRequest.InternationalBankAccount.BankName,
                                        bankAccountRequest.InternationalBankAccount.SwiftBicCode,
                                        bankAccountRequest.InternationalBankAccount.IbanNumber,
                                        bankAccountRequest.InternationalBankAccount.AccountNumber));
                                    break;
                                case FinancialAccountType.LocalBankAccount when bankAccountRequest.LocalBankAccount is not null:
                                    existingAccount.SetLocalAccount(LocalBankAccount.Create(
                                        bankAccountRequest.LocalBankAccount.AccountHolderName,
                                        bankAccountRequest.LocalBankAccount.BankName,
                                        bankAccountRequest.LocalBankAccount.CardNumber,
                                        bankAccountRequest.LocalBankAccount.ShabaNumber,
                                        bankAccountRequest.LocalBankAccount.AccountNumber));
                                    break;
                                case FinancialAccountType.Cash:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            await financialAccountRepository.UpdateAsync(existingAccount, cancellationToken);
                        }
                        else
                        {
                            var newBankAccount = FinancialAccount.CreateCustomerAccount(bankAccountRequest.FinancialAccountType,
                                new PriceUnitId(bankAccountRequest.PriceUnitId),
                                customer.Id);

                            switch (bankAccountRequest.FinancialAccountType)
                            {
                                case FinancialAccountType.InternationalBankAccount when
                                    bankAccountRequest.InternationalBankAccount is not null:
                                    newBankAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                        bankAccountRequest.InternationalBankAccount.AccountHolderName,
                                        bankAccountRequest.InternationalBankAccount.BankName,
                                        bankAccountRequest.InternationalBankAccount.SwiftBicCode,
                                        bankAccountRequest.InternationalBankAccount.IbanNumber,
                                        bankAccountRequest.InternationalBankAccount.AccountNumber));
                                    break;
                                case FinancialAccountType.LocalBankAccount when
                                    bankAccountRequest.LocalBankAccount is not null:
                                    newBankAccount.SetLocalAccount(LocalBankAccount.Create(
                                        bankAccountRequest.LocalBankAccount.AccountHolderName,
                                        bankAccountRequest.LocalBankAccount.BankName,
                                        bankAccountRequest.LocalBankAccount.CardNumber,
                                        bankAccountRequest.LocalBankAccount.ShabaNumber,
                                        bankAccountRequest.LocalBankAccount.AccountNumber));
                                    break;
                                case FinancialAccountType.Cash:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            await financialAccountRepository.CreateAsync(newBankAccount, cancellationToken);
                        }
                    }
                }

                var ledgerAccounts = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerIdSpecification(customer.Id))
                    .ToListAsync(cancellationToken);

                if (ledgerAccounts.Any())
                {
                    foreach (var ledgerAccount in ledgerAccounts)
                    {
                        ledgerAccount.SetTitle(ledgerAccount.Title.Replace(customerOldName, customer.FullName,
                            StringComparison.OrdinalIgnoreCase));
                        await ledgerAccountRepository.UpdateAsync(ledgerAccount, cancellationToken);
                    }
                }

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
}