using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نام جنس")]
    [Required(ErrorMessage = "نام جنس الزامی است")]
    public string? Name { get; set; }

    [Display(Name = "بارکد")]
    [Required(ErrorMessage = "بارکد الزامی است")]
    public string Barcode { get; set; } = default!;

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "وزن الزامی است")]
    public decimal? Weight { get; set; }

    [Display(Name = "اجرت")]
    public decimal? Wage { get; set; }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType { get; set; }

    [Display(Name = "نوع جنس")]
    public ProductType ProductType { get; set; }

    [Display(Name = "عیار")]
    public CaratType CaratType { get; set; }

    [Display(Name = "دسته بندی")]
    public Guid? ProductCategoryId { get; set; }

    public string? ProductCategoryTitle { get; set; } = string.Empty;

    [Display(Name = "واحد قیمت اجرت")]
    public Guid? WagePriceUnitId { get; set; }

    public string? WagePriceUnitTitle { get; set; }

    public ProductCategoryVm? CategoryVm { get; set; }

    public List<GemStoneVm>? Stones { get; set; }

    internal static ProductVm CreateDefaultInstance() => new()
        { CaratType = CaratType.Eighteen, ProductType = ProductType.Gold, WageType = Shared.Enums.WageType.Percent };

    internal static ProductVm CreateFrom(GetProductResponse item)
    {
        return new ProductVm
        {
            Id = item.Id,
            Name = item.Name,
            Barcode = item.Barcode,
            Weight = item.Weight,
            Wage = item.Wage,
            WageType = item.WageType,
            ProductType = item.ProductType,
            CaratType = item.CaratType,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle) ? new ProductCategoryVm
            {
                Id = item.ProductCategoryId.Value,
                Title = item.ProductCategoryTitle
            } : null,
            Stones = item.GemStones?.Select(x => new GemStoneVm
            {
                 Type = x.Type,
                 Carat = x.Carat,
                 Code = x.Code,
                 Color = x.Color,
                 Cut = x.Cut,
                 Purity = x.Purity
            }).ToList()
        };
    }

    internal static ProductRequestDto ToRequest(ProductVm item)
    {
        return new ProductRequestDto
        (
            item.Id,
            item.Name ?? string.Empty,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage ?? 0,
            item.WageType!.Value,
            item.ProductType,
            item.CaratType,
            item.ProductCategoryId,
            item.WagePriceUnitId,
            item.Stones?.Select(x => new GemStoneRequestDto(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList()
        );
    }

    public static ProductVm CreateFromSearch(GetProductResponse item)
    {
        return new ProductVm
        {
            Name = item.Name,
            Weight = item.Weight,
            Wage = item.Wage,
            WageType = item.WageType,
            ProductType = item.ProductType,
            CaratType = item.CaratType,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle)
                ? new ProductCategoryVm
                {
                    Id = item.ProductCategoryId.Value,
                    Title = item.ProductCategoryTitle
                }
                : null,
            Barcode = StringExtensions.GenerateRandomBarcode()
        };
    }
}