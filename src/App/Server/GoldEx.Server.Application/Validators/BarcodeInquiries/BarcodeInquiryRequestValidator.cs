using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Products;

namespace GoldEx.Server.Application.Validators.BarcodeInquiries;

[ScopedService]
internal class BarcodeInquiryRequestValidator : AbstractValidator<string>
{
    private readonly IProductRepository _productRepository;
    public BarcodeInquiryRequestValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x)
            .MustAsync(BeValidBarcodeAsync)
            .WithMessage("بارکد وارد شده نامعتبر است");
    }

    private Task<bool> BeValidBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return _productRepository.ExistsAsync(new ProductsByBarcodeSpecification(barcode), cancellationToken);
    }
}