using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentMethods;
using GoldEx.Shared.DTOs.PaymentMethods;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.PaymentMethods;

[ScopedService]
internal class UpdatePaymentMethodRequestValidator : AbstractValidator<(Guid id, UpdatePaymentMethodRequest request)>
{
    private readonly IPaymentMethodRepository _repository;

    public UpdatePaymentMethodRequestValidator(IPaymentMethodRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.id)
            .NotEmpty()
            .WithMessage("شناسه روش پرداخت نمی‌تواند خالی باشد.")
            .MustAsync(BeValidId)
            .WithMessage("روش پرداخت با این شناسه وجود ندارد.");

        RuleFor(x => x.request.Title)
            .NotEmpty()
            .WithMessage("عنوان روش پرداخت نمی‌تواند خالی باشد.")
            .MaximumLength(100)
            .WithMessage("طول عنوان روش پرداخت نباید بیشتر از 100 کاراکتر باشد.")
            .MustAsync(BeUniqueTitle)
            .WithMessage("روش پرداخت با این عنوان قبلاً ثبت شده است.");
    }

    private async Task<bool> BeValidId(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository
            .Get(new PaymentMethodsByIdSpecification(new PaymentMethodId(id)))
            .FirstOrDefaultAsync(cancellationToken);

        return item is not null;
    }

    private async Task<bool> BeUniqueTitle((Guid id, UpdatePaymentMethodRequest request) request, string title, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new PaymentMethodsByTitleSpecification(title))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }
}