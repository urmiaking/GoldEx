using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductVm
{
    public Guid Id { get; set; }

    [Display(Name = "نام جنس")]
    [Required(ErrorMessage = "نام جنس الزامی است")]
    public string Name { get; set; } = default!;

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
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public Guid? ProductCategoryId { get; set; }

    public string? ProductCategoryTitle { get; set; } = string.Empty;

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

    internal static CreateProductRequest ToCreateRequest(ProductVm item)
    {
        return new CreateProductRequest
        (
            Guid.NewGuid(),
            item.Name,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage ?? 0,
            item.WageType!.Value,
            item.ProductType,
            item.CaratType,
            item.ProductCategoryId!.Value, // TODO: fix this in order to handle nullability
            item.Stones?.Select(x => new CreateGemStoneRequest(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList()
        );
    }

    internal static UpdateProductRequest ToUpdateRequest(ProductVm item)
    {
        return new UpdateProductRequest
        (
            item.Name,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage ?? 0,
            item.WageType ?? 0,
            item.ProductType,
            item.CaratType,
            item.ProductCategoryId!.Value, // TODO: fix this in order to handle nullability
            item.Stones?
                .Select(x => 
                    new UpdateGemStoneRequest(x.Code, x.Type, x.Color, x.Cut, x.Carat, x.Purity))
                .ToList()
        );
    }
}