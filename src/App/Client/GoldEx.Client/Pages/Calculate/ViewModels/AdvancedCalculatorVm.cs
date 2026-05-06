using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Client.Pages.Calculate.ViewModels;

public class AdvancedCalculatorVm
{
    [Display(Name = "نام محصول")]
    public string? Name { get; set; }

    [Display(Name = "واحد ارزی")]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "واحد سنجش طلا")]
    public GoldUnitType UnitType { get; set; } = GoldUnitType.Gram;

    [Display(Name = "عیار")]
    public decimal? Fineness { get; set; }

    [Display(Name = "سود فروشنده")]
    [Required(ErrorMessage = "لطفا سود را وارد کنید")]
    public decimal ProfitPercent { get; set; } = 7;

    [Display(Name = "نرخ گرم")]
    [Required(ErrorMessage = "لطفا نرخ گرم را وارد کنید")]
    public decimal GramPrice { get; set; }

    [Display(Name = "حداکثر اجرت")]
    public decimal? MaxWage { get; set; }

    [Display(Name = "حداقل وزن")]
    public decimal? MinWeight { get; set; }

    [Display(Name = "حداکثر وزن")]
    public decimal? MaxWeight { get; set; }

    [Display(Name = "حداقل قیمت")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "حداکثر قیمت")]
    public decimal? MaxPrice { get; set; }

    public decimal TaxPercent { get; set; } = 10;

    [Display(Name = "دسته بندی")]
    public GetProductCategoryResponse? ProductCategory { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Gold;

    public void SetNull()
    {
        Name = null;
        ProductCategory = null;
        ProductType = ProductType.Gold;
        MinWeight = null;
        MaxWeight = null;
        MinPrice = null; 
        MaxPrice = null;
        MaxWage = null;
        ProfitPercent = 7;
        Fineness = null;
        ProductCategory = null;
        UnitType = GoldUnitType.Gram;
    }

    public CalculatorFilterRequest ToFilterRequest()
    {
        return new CalculatorFilterRequest(Name,
            GramPrice,
            TaxPercent,
            ProfitPercent,
            Fineness,
            MaxWage,
            MinWeight,
            MaxWeight,
            MinPrice,
            MaxPrice,
            ProductCategory?.Id,
            ProductType,
            UnitType);
    }
}