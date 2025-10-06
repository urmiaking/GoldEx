using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.DTOs.LedgerAccounts;

namespace GoldEx.Server.Application.Validators.LedgerAccounts;

[ScopedService]
internal class LedgerAccountRequestDtoValidator : AbstractValidator<LedgerAccountRequestDto>
{
    private readonly ILedgerAccountRepository _ledgerAccountRepository;

    public LedgerAccountRequestDtoValidator(ILedgerAccountRepository ledgerAccountRepository)
    {
        _ledgerAccountRepository = ledgerAccountRepository;

        RuleFor(x => x.AccountType)
            .IsInEnum()
            .WithMessage("نوع سرفصل حسابداری معتبر نیست");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان سرفصل حسابداری نمی‌تواند خالی باشد")
            .MaximumLength(100)
            .WithMessage("طول عنوان سرفصل حسابداری نباید بیشتر از 100 کاراکتر باشد");

        RuleFor(x => x.ParentAccountId)
            .MustAsync(BeValidParentAccount)
            .WithMessage("سرفصل حسابداری والد نامعتبر است");
    }

    private async Task<bool> BeValidParentAccount(Guid? parentAccountId, CancellationToken cancellationToken = default)
    {
        if (!parentAccountId.HasValue)
            return true;

        return await _ledgerAccountRepository.ExistsAsync(
            new LedgerAccountsByIdSpecification(new LedgerAccountId(parentAccountId.Value)), cancellationToken);
    }
}