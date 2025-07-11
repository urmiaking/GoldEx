﻿using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.Products;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceItemDtoValidator : AbstractValidator<InvoiceItemDto>
{
    public InvoiceItemDtoValidator(IProductCategoryRepository categoryRepository,
        IProductRepository productRepository,
        IPriceUnitRepository priceUnitRepository)
    {
        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد");

        RuleFor(i => i.GramPrice)
            .GreaterThan(0).WithMessage("نرخ گرم باید بزرگتر از صفر باشد");

        RuleFor(i => i.ProfitPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد سود نمی‌تواند منفی باشد");

        RuleFor(i => i.TaxPercent)
            .GreaterThanOrEqualTo(0).WithMessage("درصد مالیات نمی‌تواند منفی باشد");

        RuleFor(i => i.Product)
            .NotNull().WithMessage("اطلاعات محصول برای آیتم فاکتور الزامی است")
            .SetValidator(new ProductRequestDtoValidator(productRepository, categoryRepository, priceUnitRepository));
    }
}