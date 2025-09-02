using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.FinancialAccounts;

[ScopedService]
internal class FinancialAccountRequestDtoValidator : AbstractValidator<FinancialAccountRequestDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly ICustomerRepository _customerRepository;

    public FinancialAccountRequestDtoValidator(IPriceUnitRepository priceUnitRepository, ICustomerRepository customerRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _customerRepository = customerRepository;

        RuleFor(x => x.FinancialAccountType)
            .IsInEnum().WithMessage("نوع حساب بانکی نامعتبر است");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnitId)
            .WithMessage("واحد ارزی نامعتبر است");

        RuleFor(x => x.HolderName)
            .MaximumLength(100).WithMessage("حداکثر طول نام صاحب حساب 100 کاراکتر می باشد");

        RuleFor(x => x.BrokerName)
            .MaximumLength(100).WithMessage("حداکثر طول نام بانک/کارگزاری 100 کاراکتر می باشد");

        When(x => x.FinancialAccountType is FinancialAccountType.LocalBankAccount && x.LocalBankAccount is not null, () =>
        {
            RuleFor(x => x.LocalBankAccount!.CardNumber)
                .MaximumLength(20).WithMessage("حداکثر طول شماره کارت 20 کاراکتر می باشد");
            RuleFor(x => x.LocalBankAccount!.ShabaNumber)
                .MaximumLength(26).WithMessage("حداکثر طول شماره شبا 26 کاراکتر می باشد");
            RuleFor(x => x.LocalBankAccount!.AccountNumber)
                .NotEmpty().WithMessage("شماره حساب نباید خالی باشد")
                .MaximumLength(30).WithMessage("حداکثر طول شماره حساب 30 کاراکتر می باشد");
        });

        When(x => x.FinancialAccountType is FinancialAccountType.InternationalBankAccount && x.InternationalBankAccount is not null, () =>
        {
            RuleFor(x => x.InternationalBankAccount!.SwiftBicCode)
                .MaximumLength(11).WithMessage("حداکثر طول کد سوئیفت 11 کاراکتر می باشد");
            RuleFor(x => x.InternationalBankAccount!.IbanNumber)
                .MaximumLength(34).WithMessage("حداکثر طول شماره IBAN 34 کاراکتر می باشد");
            RuleFor(x => x.InternationalBankAccount!.AccountNumber)
                .NotEmpty().WithMessage("شماره حساب بین المللی نباید خالی باشد")
                .MaximumLength(30).WithMessage("حداکثر طول شماره حساب بین المللی 30 کاراکتر می باشد");
        });

        When(x => x.CustomerId.HasValue, () =>
        {
            RuleFor(x => x.CustomerId)
                .MustAsync(BeValidCustomer)
                .WithMessage("شناسه مشتری نامعتبر است");
        });

        When(x => x.CashAccount?.AccountType != CashAccountType.Internal, () =>
        {
            RuleFor(x => x.HolderName)
                .NotEmpty().WithMessage("نام صاحب حساب برای حساب های سپرده نزد دیگران الزامی است");
            RuleFor(x => x.BrokerName)
                .NotEmpty().WithMessage("نام بانک/کارگزار برای حساب های سپرده نزد دیگران الزامی است");
        });

        // TODO: Add validation for unique accounts for system and customer accounts
    }

    private async Task<bool> BeValidPriceUnitId(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)), cancellationToken);
    }
    
    private async Task<bool> BeValidCustomer(Guid? customerId, CancellationToken cancellationToken = default)
    {
        if (!customerId.HasValue)
            return false;

        return await _customerRepository.ExistsAsync(new CustomersByIdSpecification(new CustomerId(customerId.Value)), cancellationToken);
    }
}