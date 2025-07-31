using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Transactions;

namespace GoldEx.Server.Application.Validators.Customers;

[ScopedService]
internal class DeleteCustomerValidator : AbstractValidator<Customer>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    public DeleteCustomerValidator(ITransactionRepository transactionRepository, IInvoiceRepository invoiceRepository)
    {
        _transactionRepository = transactionRepository;
        _invoiceRepository = invoiceRepository;

        RuleFor(x => x)
            .MustAsync(HasNoTransactions)
            .WithMessage("مشتری دارای تراکنش است و نمی تواند حذف شود");

        RuleFor(x => x)
            .MustAsync(HasNoInvoices)
            .WithMessage("مشتری دارای فاکتور است و نمی تواند حذف شود");

        RuleFor(x => x.FinancialAccounts)
            .MustAsync(FinancialAccountsNotBeingUsed)
            .WithMessage("حساب های مالی مشتری در اسناد پرداخت استفاده شده است و نمی تواند حذف شوند");
    }

    private Task<bool> FinancialAccountsNotBeingUsed(IReadOnlyList<FinancialAccount>? financialAccounts, CancellationToken cancellationToken = default)
    {
        // TODO: implement this method to check if financial accounts are used in transactions or invoices

        //foreach (var financialAccount in financialAccounts)
        //{
        //    if (await _transactionRepository.ExistsAsync(new TransactionsByFinancialAccountIdSpecification(financialAccount.Id), cancellationToken) ||
        //        await _invoiceRepository.ExistsAsync(new InvoicesByFinancialAccountIdSpecification(financialAccount.Id), cancellationToken))
        //    {
        //        return false;
        //    }
        //}
        return Task.FromResult(true);
    }

    private async Task<bool> HasNoTransactions(Customer customer, CancellationToken cancellationToken = default)
    {
        return !await _transactionRepository.ExistsAsync(new TransactionsByCustomerIdSpecification(customer.Id), cancellationToken);
    }

    private async Task<bool> HasNoInvoices(Customer customer, CancellationToken cancellationToken = default)
    {
        return !await _invoiceRepository.ExistsAsync(new InvoicesByCustomerIdSpecification(customer.Id), cancellationToken);
    }
}