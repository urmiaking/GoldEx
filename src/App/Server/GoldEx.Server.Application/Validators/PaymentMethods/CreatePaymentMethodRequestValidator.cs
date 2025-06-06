using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentMethods;
using GoldEx.Shared.DTOs.PaymentMethods;

namespace GoldEx.Server.Application.Validators.PaymentMethods;

[ScopedService]
internal class CreatePaymentMethodRequestValidator : AbstractValidator<CreatePaymentMethodRequest>
{
    public CreatePaymentMethodRequestValidator(IPaymentMethodRepository repository)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان روش پرداخت نمی‌تواند خالی باشد.")
            .MaximumLength(100)
            .WithMessage("طول عنوان روش پرداخت نباید بیشتر از 100 کاراکتر باشد.")
            .MustAsync(async (title, cancellation) => !await repository.ExistsAsync(new PaymentMethodsByTitleSpecification(title), cancellation))
            .WithMessage("روش پرداخت با این عنوان قبلاً ثبت شده است.");
    }
}