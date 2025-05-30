using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Transactions;

[ScopedService]
internal class UpdateTransactionRequestValidator: AbstractValidator<(Guid id, UpdateTransactionRequest request)>
{
    private readonly ITransactionRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    public UpdateTransactionRequestValidator(ITransactionRepository repository, ICustomerRepository customerRepository)
    {
        _repository = repository;
        _customerRepository = customerRepository;

        RuleFor(x => x.id)
            .NotEmpty().WithMessage("شناسه تراكنش نمي تواند خالی باشد")
            .MustAsync(BeValidId).WithMessage("شناسه تراكنش نامعتبر است");

        RuleFor(x => x.request.Number)
            .NotEmpty().WithMessage("شماره تراكنش نمی تواند خالی باشد")
            .GreaterThan(0).WithMessage("شماره تراكنش نمی تواند منفی باشد")
            .MustAsync(BeUniqueNumber).WithMessage("شماره تراكنش تكراری است");

        RuleFor(x => x.request.Description)
            .NotEmpty().WithMessage("شرح تراكنش نمی تواند خالی باشد");

        RuleFor(transaction => transaction.request)
            .Must(HaveAtLeastCreditOrDebitInfo)
            .WithMessage("وارد کردن حداقل یکی از مقادیر بدهکاری یا بستانکاری الزامی است");

        RuleFor(x => x.request.CustomerId)
            .NotEmpty().WithMessage("مشتری مشخص نشده است")
            .MustAsync(BeValidCustomer).WithMessage("مشتری نامعتبر است");

        When(transaction => transaction.request.Credit.HasValue, () => {
            RuleFor(transaction => transaction.request.CreditUnit)
                .NotNull().WithMessage("وارد کردن واحد تبدیل بستانکاری الزامی است");

            When(transaction => transaction.request.CreditUnit is not UnitType.IRR, () =>
            {
                RuleFor(transaction => transaction.request.CreditRate)
                    .NotNull().WithMessage("وارد کردن نرخ تبدیل بستانکاری الزامی است")
                    .GreaterThanOrEqualTo(0).WithMessage("نرخ تبدیل بستانکاری نباید منفی باشد");
            });

            RuleFor(transaction => transaction.request.Credit)
                 .GreaterThan(0).WithMessage("مقدار بستانکاری نباید منفی باشد");
        });

        When(transaction => transaction.request.Debit.HasValue, () => {
            RuleFor(transaction => transaction.request.DebitUnit)
                .NotNull().WithMessage("وارد کردن واحد تبدیل بدهکاری الزامی است");

            When(transaction => transaction.request.DebitUnit is not UnitType.IRR, () =>
            {
                RuleFor(transaction => transaction.request.DebitRate)
                    .NotNull().WithMessage("وارد کردن نرخ تبدیل بدهکاری الزامی است")
                    .GreaterThanOrEqualTo(0).WithMessage("نرخ تبدیل بدهکاری نباید منفی باشد");
            });

            RuleFor(transaction => transaction.request.Debit)
                .GreaterThan(0).WithMessage("مقدار بدهکاری نباید منفی باشد");
        });
    }

    private async Task<bool> BeValidCustomer(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.ExistsAsync(new CustomersByIdSpecification(new CustomerId(customerId)), cancellationToken);
    }

    private async Task<bool> BeUniqueNumber((Guid id, UpdateTransactionRequest request) request, long number, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new TransactionsByNumberSpecification(number))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }

    private static bool HaveAtLeastCreditOrDebitInfo(UpdateTransactionRequest transaction)
    {
        return transaction.Credit.HasValue || transaction.Debit.HasValue;
    }

    private async Task<bool> BeValidId(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(new TransactionsByIdSpecification(new TransactionId(id)), cancellationToken);
    }
}