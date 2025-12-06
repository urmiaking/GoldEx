using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.FinancialAccounts;

[ScopedService]
internal class FinancialAccountRequestDtoValidator : AbstractValidator<FinancialAccountRequestDto>
{
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IFinancialAccountRepository _repository;

    public FinancialAccountRequestDtoValidator(IPriceUnitRepository priceUnitRepository, ICustomerRepository customerRepository, IFinancialAccountRepository repository)
    {
        _priceUnitRepository = priceUnitRepository;
        _customerRepository = customerRepository;
        _repository = repository;

        RuleFor(x => x.FinancialAccountType)
            .IsInEnum().WithMessage("نوع حساب بانکی نامعتبر است");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnitId).WithMessage("واحد ارزی نامعتبر است");

        RuleFor(x => x.CustomerId)
            .MustAsync(BeValidCustomer).WithMessage("شناسه مشتری نامعتبر است");

        RuleFor(x => x.LocalBankAccount)
            .NotNull().WithMessage("اطلاعات حساب بانکی داخلی الزامی است.")
            .When(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount);

        RuleFor(x => x.InternationalBankAccount)
            .NotNull().WithMessage("اطلاعات حساب بانکی بین المللی الزامی است.")
            .When(x => x.FinancialAccountType == FinancialAccountType.InternationalBankAccount);

        When(x => x.FinancialAccountType != FinancialAccountType.Gold &&
                  x is not { FinancialAccountType: FinancialAccountType.Cash, CashAccount.AccountType: CashAccountType.Internal }, () =>
        {
            RuleFor(x => x.HolderName)
                .NotEmpty()
                .WithMessage(dto => dto.FinancialAccountType == FinancialAccountType.Cash 
                    ? "نام صاحب حساب برای حساب های سپرده نزد دیگران الزامی است"
                    : "نام صاحب حساب نباید خالی باشد.")
                .MaximumLength(100).WithMessage("نام صاحب حساب نباید بیشتر از 100 کاراکتر باشد.");

            RuleFor(x => x.BrokerName)
                .NotEmpty()
                .WithMessage(dto => dto.FinancialAccountType == FinancialAccountType.Cash
                    ? "نام بانک/کارگزار برای حساب های سپرده نزد دیگران الزامی است"
                    : "نام بانک/کارگزار نباید خالی باشد.")
                .MaximumLength(100).WithMessage("نام بانک/کارگزار نباید بیشتر از 100 کاراکتر باشد.");
        });

        When(x => x.FinancialAccountType is FinancialAccountType.Gold, () =>
        {
            RuleFor(x => x)
                .MustAsync(BeUniqueGoldAccount)
                .WithMessage("تنها یک حساب طلایی می تواند وجود داشته باشد.");
        });

        RuleFor(x => x.LocalBankAccount)
            .SetValidator(new LocalBankAccountRequestDtoValidator()!)
            .When(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount && x.LocalBankAccount is not null);

        RuleFor(x => x.InternationalBankAccount)
            .SetValidator(new InternationalBankAccountRequestDtoValidator()!)
            .When(x => x.FinancialAccountType == FinancialAccountType.InternationalBankAccount && x.InternationalBankAccount is not null);
    }

    /// <summary>
    /// Only one system gold account can exist or one gold account per customer
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> BeUniqueGoldAccount(FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request is { IsSystemAccount: false, CustomerId: null })
        {
            // if the gold account is not a system account, and we don't have a customer id, it means that a new customer request is being created so we can allow it
            return true;
        }
        // also if we are updating an existing gold account, we can allow it
        if (request.Id.HasValue)
            return true;

        return !await _repository.ExistsAsync(new FinancialAccountsByTypeSpecification(request.FinancialAccountType, request.CustomerId), cancellationToken);
    }

    private async Task<bool> BeValidPriceUnitId(Guid priceUnitId, CancellationToken cancellationToken)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)), cancellationToken);
    }

    private async Task<bool> BeValidCustomer(Guid? customerId, CancellationToken cancellationToken)
    {
        if (!customerId.HasValue)
            return true;

        return await _customerRepository.ExistsAsync(new CustomersByIdSpecification(new CustomerId(customerId.Value)), cancellationToken);
    }
}

public class LocalBankAccountRequestDtoValidator : AbstractValidator<LocalBankAccountRequestDto>
{
    public LocalBankAccountRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(AtLeastOneLocalFieldProvided)
            .WithMessage("حداقل یکی از فیلدهای شماره حساب، شماره کارت یا شماره شبا باید پر شود.");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(20).WithMessage("شماره حساب نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.CardNumber)
            .MaximumLength(20).WithMessage("شماره کارت نباید بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.ShabaNumber)
            .MaximumLength(40).WithMessage("شماره شبا نباید بیشتر از 40 کاراکتر باشد.");
    }

    private bool AtLeastOneLocalFieldProvided(LocalBankAccountRequestDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.AccountNumber) ||
               !string.IsNullOrWhiteSpace(dto.CardNumber) ||
               !string.IsNullOrWhiteSpace(dto.ShabaNumber);
    }
}

public class InternationalBankAccountRequestDtoValidator : AbstractValidator<InternationalBankAccountRequestDto>
{
    public InternationalBankAccountRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(AtLeastOneInternationalFieldProvided)
            .WithMessage("حداقل یکی از فیلدهای شماره حساب، کد سوئیفت یا شماره IBAN باید پر شود.");

        RuleFor(x => x.AccountNumber)
            .MaximumLength(30).WithMessage("شماره حساب نباید بیشتر از 30 کاراکتر باشد.");

        RuleFor(x => x.SwiftBicCode)
            .MaximumLength(11).WithMessage("کد سوئیفت نباید بیشتر از 11 کاراکتر باشد.");

        RuleFor(x => x.IbanNumber)
            .MaximumLength(34).WithMessage("شماره IBAN نباید بیشتر از 34 کاراکتر باشد.");
    }

    private bool AtLeastOneInternationalFieldProvided(InternationalBankAccountRequestDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.AccountNumber) ||
               !string.IsNullOrWhiteSpace(dto.SwiftBicCode) ||
               !string.IsNullOrWhiteSpace(dto.IbanNumber);
    }
}