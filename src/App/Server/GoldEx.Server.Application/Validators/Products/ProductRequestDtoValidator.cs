using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Products;

[ScopedService]
internal class ProductRequestDtoValidator : AbstractValidator<ProductRequestDto>
{
    private readonly IProductRepository _repository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    public ProductRequestDtoValidator(IProductRepository repository,
        IPriceUnitRepository priceUnitRepository)
    {
        _repository = repository;
        _priceUnitRepository = priceUnitRepository;

        RuleFor(x => x.Name)
            .NotEmpty().When(x => x.ProductType is not ProductType.MoltenGold).WithMessage("عنوان جنس نمی تواند خالی باشد")
            .MaximumLength(50).WithMessage("طول عنوان جنس نمی تواند بیشتر از 50 کاراکتر باشد");

        When(x => x.WageType is WageType.Fixed, () =>
        {
            RuleFor(product => product.WagePriceUnitId)
                .NotNull().WithMessage("لطفا واحد قیمت اجرت را وارد کنید")
                .NotEmpty().WithMessage("واحد قیمت اجرت نمی تواند خالی باشد")
                .MustAsync(BeValidWagePriceId)
                .WithMessage("واحد قیمت اجرت وارد شده معتبر نیست");
        });

        RuleFor(x => x.ProductType).IsInEnum().WithMessage("لطفا نوع جنس را انتخاب کنید");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000)
            .WithMessage("عیار باید بین 0 تا 1000 باشد");

        RuleFor(x => x.Barcode)
            .MustAsync(BeUniqueBarcode)
            .WithMessage((dto, barcode) => $"بارکد '{barcode}' برای جنس '{dto.Name}' .تکراری است");

        RuleFor(x => x.Id)
            .MustAsync(BeValidId)
            .WithMessage("شناسه جنس وارد شده معتبر نیست");
    }

    private async Task<bool> BeUniqueBarcode(ProductRequestDto request, string? barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(barcode))
            return true;

        var item = await _repository
            .Get(new ProductsByBarcodeSpecification(barcode))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return item is null || item.Id.Value == request.Id;
    }

    private async Task<bool> BeValidId(Guid? id, CancellationToken cancellationToken = default)
    {
        if (!id.HasValue)
            return true;

        return await _repository.ExistsAsync(new ProductsByIdSpecification(new ProductId(id.Value)), cancellationToken);
    }

    private async Task<bool> BeValidWagePriceId(Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        if (!priceUnitId.HasValue)
            return true;

        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)), cancellationToken);
    }
}