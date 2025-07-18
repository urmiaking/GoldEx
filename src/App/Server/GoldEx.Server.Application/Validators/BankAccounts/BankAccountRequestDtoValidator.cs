using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.BankAccounts;

namespace GoldEx.Server.Application.Validators.BankAccounts;

[ScopedService]
internal class BankAccountRequestDtoValidator : AbstractValidator<BankAccountRequestDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;

    public BankAccountRequestDtoValidator(IPriceUnitRepository priceUnitRepository)
    {
        _priceUnitRepository = priceUnitRepository;

        RuleFor(x => x.BankAccountType)
            .IsInEnum().WithMessage("نوع حساب بانکی نامعتبر است");

        RuleFor(x => x.AccountHolderName)
            .NotEmpty().WithMessage("وارد کردن نام صاحب حساب الزامی است")
            .MaximumLength(100).WithMessage("حداکثر طول نام صاحب حساب 100 کاراکتر می باشد");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("وارد کردن نام بانک الزامی است")
            .MaximumLength(100).WithMessage("حداکثر طول نام بانک 100 کاراکتر می باشد");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnitId)
            .WithMessage("واحد ارزی نامعتبر است");

        When(x => x.LocalBankAccount is not null, () =>
        {
            RuleFor(x => x.LocalBankAccount!.CardNumber)
                .MaximumLength(20).WithMessage("حداکثر طول شماره کارت 20 کاراکتر می باشد");
            RuleFor(x => x.LocalBankAccount!.ShabaNumber)
                .MaximumLength(26).WithMessage("حداکثر طول شماره شبا 26 کاراکتر می باشد");
            RuleFor(x => x.LocalBankAccount!.AccountNumber)
                .MaximumLength(30).WithMessage("حداکثر طول شماره حساب 30 کاراکتر می باشد");
        });

        When(x => x.InternationalBankAccount is not null, () =>
        {
            RuleFor(x => x.InternationalBankAccount!.SwiftBicCode)
                .MaximumLength(11).WithMessage("حداکثر طول کد سوئیفت 11 کاراکتر می باشد");
            RuleFor(x => x.InternationalBankAccount!.IbanNumber)
                .MaximumLength(34).WithMessage("حداکثر طول شماره IBAN 34 کاراکتر می باشد");
            RuleFor(x => x.InternationalBankAccount!.AccountNumber)
                .MaximumLength(30).WithMessage("حداکثر طول شماره حساب بین المللی 30 کاراکتر می باشد");
        });
    }

    private async Task<bool> BeValidPriceUnitId(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)), cancellationToken);
    }
}