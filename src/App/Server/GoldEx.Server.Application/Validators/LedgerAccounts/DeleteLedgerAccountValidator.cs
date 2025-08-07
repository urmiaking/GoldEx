using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

namespace GoldEx.Server.Application.Validators.LedgerAccounts;

[ScopedService]
internal class DeleteLedgerAccountValidator : AbstractValidator<LedgerAccount>
{
    private readonly ILedgerAccountRepository _ledgerAccountRepository;
    private readonly IFinancialAccountRepository _financialAccountRepository;

    public DeleteLedgerAccountValidator(ILedgerAccountRepository ledgerAccountRepository, IFinancialAccountRepository financialAccountRepository)
    {
        _ledgerAccountRepository = ledgerAccountRepository;
        _financialAccountRepository = financialAccountRepository;

        RuleFor(x => x)
            .MustAsync(HasNoChildAccounts)
            .WithMessage("این سرفصل به دلیل داشتن زیر سرفصل قابل حذف نمی باشد.")
            .MustAsync(NotUsedInFinancialAccounts)
            .WithMessage("این سرفصل در حساب های مالی استفاده شده است و قابل حذف نمی باشد");
    }

    private async Task<bool> NotUsedInFinancialAccounts(LedgerAccount ledgerAccount, CancellationToken cancellationToken = default)
    {
        return !await _financialAccountRepository.ExistsAsync(new FinancialAccountsByLedgerAccountIdSpecification(ledgerAccount.Id), cancellationToken);
    }

    private async Task<bool> HasNoChildAccounts(LedgerAccount ledgerAccount, CancellationToken cancellationToken)
    {
        return !await _ledgerAccountRepository.ExistsAsync(new LedgerAccountsByParentIdSpecification(ledgerAccount.Id), cancellationToken);
    }
}