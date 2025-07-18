using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Domain.BankAccountAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
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
    IBankAccountRepository bankAccountRepository,
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
            .Include(x => x.BankAccounts!)
                .ThenInclude(x => x.PriceUnit)
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
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null ? mapper.Map<GetCustomerResponse>(item) : null;
    }

    public async Task<GetCustomerResponse?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CustomersByPhoneNumberSpecification(phoneNumber))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null ? mapper.Map<GetCustomerResponse>(item) : null;
    }

    public async Task<Guid> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        try
        {
            var customer = Customer.Create(request.CustomerType,
                request.FullName,
                request.NationalId,
                request.PhoneNumber,
                request.Address,
                request.CreditLimit,
                request.CreditLimitPriceUnitId.HasValue ? new PriceUnitId(request.CreditLimitPriceUnitId.Value) : null);

            await repository.CreateAsync(customer, cancellationToken);

            List<BankAccount> bankAccounts = [];

            if (request.BankAccounts is not null)
            {
                foreach (var bankAccountRequest in request.BankAccounts)
                {
                    var bankAccount = BankAccount.Create(bankAccountRequest.BankAccountType,
                        bankAccountRequest.AccountHolderName,
                        bankAccountRequest.BankName,
                        new PriceUnitId(bankAccountRequest.PriceUnitId),
                        customer.Id);

                    switch (bankAccount.AccountType)
                    {
                        case BankAccountType.International when bankAccountRequest.InternationalBankAccount is not null:
                            bankAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                bankAccountRequest.InternationalBankAccount.SwiftBicCode,
                                bankAccountRequest.InternationalBankAccount.IbanNumber,
                                bankAccountRequest.InternationalBankAccount.AccountNumber));
                            break;
                        case BankAccountType.Local when
                            bankAccountRequest.LocalBankAccount is not null:
                            bankAccount.SetLocalAccount(LocalBankAccount.Create(
                                bankAccountRequest.LocalBankAccount.CardNumber,
                                bankAccountRequest.LocalBankAccount.ShabaNumber,
                                bankAccountRequest.LocalBankAccount.AccountNumber));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    bankAccounts.Add(bankAccount);
                }

                await bankAccountRepository.CreateRangeAsync(bankAccounts, cancellationToken);
            }

            return customer.Id.Value;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        try
        {
            var customer = await repository
                .Get(new CustomersByIdSpecification(new CustomerId(id)))
                .Include(x => x.BankAccounts)
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

            customer.SetFullName(request.FullName);
            customer.SetNationalId(request.NationalId);
            customer.SetPhoneNumber(request.PhoneNumber);
            customer.SetAddress(request.Address);
            customer.SetCustomerType(request.CustomerType);
            customer.SetCreditLimit(request.CreditLimit,
                request.CreditLimitPriceUnitId.HasValue
                    ? new PriceUnitId(request.CreditLimitPriceUnitId.Value)
                    : null);

            if (request.BankAccounts is not null)
            {
                var existingBankAccounts = customer.BankAccounts?.ToList() ?? [];
                // Remove bank accounts that are not in the request
                foreach (var existingAccount in existingBankAccounts)
                {
                    if (request.BankAccounts.All(x => x.Id != existingAccount.Id.Value))
                    {
                        await bankAccountRepository.DeleteAsync(existingAccount, cancellationToken);
                    }
                }

                // Update or add new bank accounts
                foreach (var bankAccountRequest in request.BankAccounts)
                {
                    var existingAccount =
                        existingBankAccounts.FirstOrDefault(x => x.Id.Value == bankAccountRequest.Id);
                    if (existingAccount is not null)
                    {
                        existingAccount.SetAccountType(bankAccountRequest.BankAccountType);
                        existingAccount.SetAccountHolderName(bankAccountRequest.AccountHolderName);
                        existingAccount.SetBankName(bankAccountRequest.BankName);
                        existingAccount.SetPriceUnitId(new PriceUnitId(bankAccountRequest.PriceUnitId));

                        switch (existingAccount.AccountType)
                        {
                            case BankAccountType.International when bankAccountRequest.InternationalBankAccount is not null:
                                existingAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                    bankAccountRequest.InternationalBankAccount.SwiftBicCode,
                                    bankAccountRequest.InternationalBankAccount.IbanNumber,
                                    bankAccountRequest.InternationalBankAccount.AccountNumber));
                                break;
                            case BankAccountType.Local when bankAccountRequest.LocalBankAccount is not null:
                                existingAccount.SetLocalAccount(LocalBankAccount.Create(
                                    bankAccountRequest.LocalBankAccount.CardNumber,
                                    bankAccountRequest.LocalBankAccount.ShabaNumber,
                                    bankAccountRequest.LocalBankAccount.AccountNumber));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        await bankAccountRepository.UpdateAsync(existingAccount, cancellationToken);
                    }
                    else
                    {
                        var newBankAccount = BankAccount.Create(bankAccountRequest.BankAccountType,
                            bankAccountRequest.AccountHolderName,
                            bankAccountRequest.BankName,
                            new PriceUnitId(bankAccountRequest.PriceUnitId),
                            customer.Id);

                        switch (bankAccountRequest.BankAccountType)
                        {
                            case BankAccountType.International when
                                bankAccountRequest.InternationalBankAccount is not null:
                                newBankAccount.SetInternationalAccount(InternationalBankAccount.Create(
                                    bankAccountRequest.InternationalBankAccount.SwiftBicCode,
                                    bankAccountRequest.InternationalBankAccount.IbanNumber,
                                    bankAccountRequest.InternationalBankAccount.AccountNumber));
                                break;
                            case BankAccountType.Local when
                                bankAccountRequest.LocalBankAccount is not null:
                                newBankAccount.SetLocalAccount(LocalBankAccount.Create(
                                    bankAccountRequest.LocalBankAccount.CardNumber,
                                    bankAccountRequest.LocalBankAccount.ShabaNumber,
                                    bankAccountRequest.LocalBankAccount.AccountNumber));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        await bankAccountRepository.CreateAsync(newBankAccount, cancellationToken);
                    }
                }
            }

            await repository.UpdateAsync(customer, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new CustomersByIdSpecification(new CustomerId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

        await repository.DeleteAsync(item, cancellationToken);
    }
}