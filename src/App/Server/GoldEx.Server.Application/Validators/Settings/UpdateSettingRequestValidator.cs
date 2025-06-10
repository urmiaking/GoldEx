using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Server.Application.Validators.Settings;

[ScopedService]
internal class UpdateSettingRequestValidator : AbstractValidator<UpdateSettingRequest>
{
    public UpdateSettingRequestValidator()
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

        RuleFor(x => x.TaxPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد مالیات باید بین 0 تا 100 باشد");

        RuleFor(x => x.GoldProfitPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد سود طلا باید بین 0 تا 100 باشد");

        RuleFor(x => x.JewelryProfitPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد سود جواهرات باید بین 0 تا 100 باشد");

        RuleFor(x => x.GoldSafetyMarginPercent)
            .InclusiveBetween(0, 100).WithMessage("درصد حاشیه ایمن طلا باید بین 0 تا 100 باشد.");

        RuleFor(x => x.PriceUpdateInterval)
            .NotEmpty().WithMessage("فاصله زمانی بروزرسانی قیمت نمی تواند خالی باشد");
    }
}