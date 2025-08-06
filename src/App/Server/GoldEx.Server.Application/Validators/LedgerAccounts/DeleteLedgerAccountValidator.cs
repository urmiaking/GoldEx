using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

namespace GoldEx.Server.Application.Validators.LedgerAccounts;

[ScopedService]
internal class DeleteLedgerAccountValidator : AbstractValidator<LedgerAccount>
{
    private readonly ILedgerAccountRepository _ledgerAccountRepository;

    public DeleteLedgerAccountValidator(ILedgerAccountRepository ledgerAccountRepository)
    {
        _ledgerAccountRepository = ledgerAccountRepository;

        RuleFor(x => x)
            .MustAsync(HasNoChildAccounts)
            .WithMessage("این سرفصل به دلیل داشتن زیر سرفصل قابل حذف نمی باشد.");
    }

    private async Task<bool> HasNoChildAccounts(LedgerAccount ledgerAccount, CancellationToken cancellationToken)
    {
        return !await _ledgerAccountRepository.ExistsAsync(new LedgerAccountsByParentIdSpecification(ledgerAccount.Id), cancellationToken);
    }
}