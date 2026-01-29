using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Licenses;

namespace GoldEx.Server.Application.Validators.Licenses;

[ScopedService]
internal class RegisterProductRequestValidator : AbstractValidator<RegisterProductRequest>
{
    public RegisterProductRequestValidator()
    {
        RuleFor(x => x.InstitutionName)
            .NotEmpty().WithMessage("نام گالری نمی تواند خالی باشد")
            .MaximumLength(25).WithMessage("طول نام گالری نمی تواند بیشتر از 25 کاراکتر باشد");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("آدرس نمی تواند خالی باشد")
            .MaximumLength(250).WithMessage("طول آدرس نمی تواند بیشتر از 250 کاراکتر باشد");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تماس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول شماره تماس نمی تواند بیشتر از 50 کاراکتر باشد");
    }
}