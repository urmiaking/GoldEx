using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
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
    public double? Weight { get; set; }

    [Display(Name = "اجرت")]
    public double? Wage { get; set; }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType { get; set; }

    [Display(Name = "نوع کالا")]
    public ProductType ProductType { get; set; }

    [Display(Name = "عیار")]
    public CaratType CaratType { get; set; }

    internal static ProductVm CreateDefaultInstance() => new()
        { CaratType = CaratType.Eighteen, ProductType = ProductType.Gold, WageType = GoldEx.Shared.Enums.WageType.Percent };

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
            CaratType = item.CaratType
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
            item.Wage,
            item.WageType,
            item.ProductType,
            item.CaratType
        );
    }

    internal static UpdateProductRequest ToUpdateRequest(ProductVm item)
    {
        return new UpdateProductRequest
        (
            item.Name,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage,
            item.WageType,
            item.ProductType,
            item.CaratType
        );
    }
}