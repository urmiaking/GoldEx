using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductVm
{
    public Guid Id { get; set; }

    [Display(Name = "نام جنس")]
    public string Name { get; set; } = default!;

    [Display(Name = "بارکد")]
    public string Barcode { get; set; } = default!;

    [Display(Name = "وزن")]
    public double Weight { get; set; }

    [Display(Name = "اجرت")]
    public double? Wage { get; set; }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType { get; set; } = default!;

    [Display(Name = "نوع کالا")]
    public ProductType ProductType { get; set; } = default!;

    [Display(Name = "عیار")]
    public CaratType CaratType { get; set; } = default!;


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
}