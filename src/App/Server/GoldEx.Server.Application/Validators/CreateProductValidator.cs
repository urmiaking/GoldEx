﻿using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators;

[ScopedService]
public class CreateProductValidator : AbstractValidator<Product>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("بارکد جنس نمی تواند خالی باشد");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("لطفا وزن جنس را وارد کنید");

        RuleFor(x => x.CreatedUserId)
            .NotEqual(Guid.Empty)
            .WithMessage("کاربر نامعتبر");

        When(product => product.ProductType == ProductType.Gold, () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("اجرت ساخت برای طلا الزامی است");
            RuleFor(product => product.WageType)
                .Must(wageType => wageType is WageType.Percent or WageType.Rial)
                .WithMessage("نوع اجرت برای طلا، فقط میتواند ریالی یا درصدی باشد");
        });

        When(product => product.ProductType == ProductType.Jewelry, () =>
        {
            RuleFor(product => product.Wage).NotNull().WithMessage("اجرت ساخت برای جواهر الزامی است");
            RuleFor(product => product.WageType)
                .Must(wageType => wageType is WageType.Dollar or WageType.Rial)
                .WithMessage("نوع اجرت برای جواهر، فقط می تواند دلاری یا ریالی باشد");
        });

        When(product => product.ProductType != ProductType.Gold && product.ProductType != ProductType.Jewelry, () =>
        {
            RuleFor(product => product.Wage).Null().WithMessage("اجرت ساخت برای سکه ها، طلای آبشده و کهنه نباید وارد شود");
            RuleFor(product => product.WageType).Null().WithMessage("نوع اجرت برای سکه ها، طلای آبشده و کهنه نباید وارد شود");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.CaratType).IsInEnum().WithMessage("لطفا عیار را انتخاب کنید");
    }
}