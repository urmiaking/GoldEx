using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Server.Application.Validators.Settings;

[ScopedService]
internal class CreateSettingRequestValidator : AbstractValidator<CreateSettingRequest>
{
    public CreateSettingRequestValidator()
    {
        RuleFor(x => x.InstitutionName)
            .NotEmpty().WithMessage("نام گالری نمی تواند خالی باشد.")
            .MaximumLength(100).WithMessage("نام گالری باید کمتر از 100 کاراکتر باشد.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("آدرس نمی تواند خالی باشد.")
            .MaximumLength(200).WithMessage("آدرس باید کمتر از 200 کاراکتر باشد.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن نمی تواند خالی باشد.");

        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد مالیات باید بین 0 تا 100 باشد.");

        RuleFor(x => x.GoldProfitPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد سود طلا باید بین 0 تا 100 باشد.");

        RuleFor(x => x.JewelryProfitPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد سود جواهرات باید بین 0 تا 100 باشد.");

        RuleFor(x => x.GoldSafetyMarginPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد حاشیه ایمن طلا باید بین 0 تا 100 باشد.");
    }
}