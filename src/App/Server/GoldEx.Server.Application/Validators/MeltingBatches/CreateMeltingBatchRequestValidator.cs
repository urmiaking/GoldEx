using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.MeltingBatches;

[ScopedService]
internal class CreateMeltingBatchRequestValidator : AbstractValidator<MeltingBatchRequestDto>
{
    private readonly IProductRepository _productRepository;
    public CreateMeltingBatchRequestValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("توضیحات الزامی است.")
            .MaximumLength(200).WithMessage("توضیحات نباید بیشتر از 200 کاراکتر باشد.");

        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("حداقل یک محصول باید انتخاب شود.")
            .MustAsync(BeValidProducts)
            .WithMessage("یک یا چندتا از محصولات انتخاب شده معتبر نیستند.");
    }

    private async Task<bool> BeValidProducts(List<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository
            .Get(new ProductsByIdsSpecification(productIds.Select(x => new ProductId(x)).ToList()))
            .ToListAsync(cancellationToken);

        if (products.Any(product => product.ProductType is not ProductType.UsedGold))
            return false;

        return products.Count == productIds.Count;
    }
}