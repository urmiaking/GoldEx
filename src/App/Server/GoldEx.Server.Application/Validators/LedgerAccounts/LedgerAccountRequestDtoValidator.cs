using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.DTOs.LedgerAccounts;

namespace GoldEx.Server.Application.Validators.LedgerAccounts;

[ScopedService]
internal class LedgerAccountRequestDtoValidator : AbstractValidator<LedgerAccountRequestDto>
{
    private readonly ILedgerAccountRepository _ledgerAccountRepository;
    private readonly ICustomerRepository _customerRepository;
    public LedgerAccountRequestDtoValidator(ILedgerAccountRepository ledgerAccountRepository, ICustomerRepository customerRepository)
    {
        _ledgerAccountRepository = ledgerAccountRepository;
        _customerRepository = customerRepository;

        RuleFor(x => x.AccountType)
            .IsInEnum()
            .WithMessage("نوع سرفصل حسابداری معتبر نیست");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان سرفصل حسابداری نمی‌تواند خالی باشد")
            .MaximumLength(100)
            .WithMessage("طول عنوان سرفصل حسابداری نباید بیشتر از 100 کاراکتر باشد");

        When(x => x.IsSystemAccount, () =>
        {
            RuleFor(x => x.CustomerId)
                .Null()
                .WithMessage("سرفصل حسابداری سیستم نباید مشتری داشته باشد");
        });
        When(x => !x.IsSystemAccount, () =>
        {
            RuleFor(x => x.CustomerId)
                .NotNull()
                .WithMessage("سرفصل حسابداری مشتری باید مشخص شود");

            RuleFor(x => x.CustomerId)
                .MustAsync(BeValidCustomer)
                .WithMessage("مشتری نامعتبر است");
        });

        RuleFor(x => x.ParentAccountId)
            .MustAsync(BeValidParentAccount)
            .WithMessage("سرفصل حسابداری والد نامعتبر است");
    }

    private async Task<bool> BeValidCustomer(Guid? customerId, CancellationToken cancellationToken)
    {
        if (!customerId.HasValue)
            return true;

        return await _customerRepository.ExistsAsync(new CustomersByIdSpecification(new CustomerId(customerId.Value)), cancellationToken);
    }

    private async Task<bool> BeValidParentAccount(Guid? parentAccountId, CancellationToken cancellationToken = default)
    {
        if (!parentAccountId.HasValue)
            return true;

        return await _ledgerAccountRepository.ExistsAsync(
            new LedgerAccountsByIdSpecification(new LedgerAccountId(parentAccountId.Value)), cancellationToken);
    }
}