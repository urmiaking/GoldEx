using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class LedgerAccountRepository(
    GoldExDbContext dbContext,
    ICustomerRepository customerRepository) : RepositoryBase<LedgerAccount>(dbContext),
    ILedgerAccountRepository
{
    public async Task CreateForCustomerAsync(CustomerId customerId, string parentAccountTitle,
        CancellationToken cancellationToken = default)
    {
        var parentAccount = await Query.Where(x => x.Title == parentAccountTitle)
            .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

        var existingLedgerAccount = await Query
            .Where(x => x.CustomerId == customerId && x.ParentAccountId == parentAccount.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingLedgerAccount != null)
            return;

        var customer = await customerRepository
                           .Get(new CustomersByIdSpecification(customerId))
                           .AsNoTracking()
                           .FirstOrDefaultAsync(cancellationToken) 
                       ?? throw new InvalidOperationException($"Customer with ID '{customerId.Value}' not found.");

        var ledgerAccountTitle = $"{parentAccount.Title} - {customer.FullName}";

        var ledgerAccount = LedgerAccount.CreateCustomerAccount(ledgerAccountTitle, customerId, parentAccount.AccountType, parentAccount.Id);
        await CreateAsync(ledgerAccount, cancellationToken);
    }
}