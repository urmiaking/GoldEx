using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class UsedProductVm
{
    private decimal _gramPrice;
    private decimal? _fineness = 735m;
    private decimal? _weight;
    private decimal? _exchangeRate;

    [Display(Name = "نرخ هر گرم طلا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal GramPrice
    {
        get => _gramPrice;
        set
        {
            _gramPrice = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "عیار")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    [Range(0, 1000, ErrorMessage = "{0} باید بین {1} و {2} باشد.")]
    public decimal? Fineness
    {
        get => _fineness;
        set
        {
            _fineness = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal? Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "نرخ تبدیل ارز")]
    public decimal? ExchangeRate
    {
        get => _exchangeRate;
        set
        {
            _exchangeRate = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "عنوان/توضیحات")]
    [MaxLength(100, ErrorMessage = "{0} نمی تواند بیشتر از {1} کاراکتر باشد.")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Description { get; set; }

    [Display(Name = "افزودن به انبار")]
    public bool IsSellable { get; set; }

    [Display(Name = "هزینه های جانبی")]
    public decimal? ExtraCostsAmount { get; set; }

    [Display(Name = "مبلغ نهایی")]
    public decimal ItemFinalAmount => ItemAmount + (ExtraCostsAmount ?? 0);

    [Display(Name = "مبلغ کالا")]
    public decimal ItemAmount { get; set; }

    [Display(Name = "نوع کالا")]
    public ProductType ProductType { get; set; } = ProductType.Gold;

    [Display(Name = "نوع واحد طلا")]
    public GoldUnitType UnitType { get; set; }

    public int Index { get; set; } = 1;

    /// <summary>
    /// This method performs the client-side calculation and updates the display properties.
    /// It's called whenever an input property changes.
    /// </summary>
    public void RecalculateAmounts()
    {
        if (Weight == null || Fineness == null || GramPrice <= 0)
        {
            ItemAmount = 0;
            return;
        }

        ItemAmount = CalculatorHelper.UsedProduct.Calculate(Weight.Value, Fineness.Value, GramPrice, ExchangeRate);
    }

    public void UpdateFrom(UsedProductVm other)
    {
        Index = other.Index;
        GramPrice = other.GramPrice;
        ExchangeRate = other.ExchangeRate;
        Fineness = other.Fineness;
        Weight = other.Weight;
        Description = other.Description;
        IsSellable = other.IsSellable;
        ExtraCostsAmount = other.ExtraCostsAmount;
        ProductType = other.ProductType;
        UnitType = other.UnitType;
    }

    public static InvoiceUsedProductDto ToRequest(UsedProductVm productItem)
    {
        return new InvoiceUsedProductDto(productItem.Description ?? string.Empty,
            productItem.Weight ?? 0,
            productItem.GramPrice,
            productItem.ExtraCostsAmount ?? 0,
            productItem.Fineness ?? 0,
            productItem.IsSellable,
            productItem.ProductType,
            productItem.UnitType);
    }

    //public static ProductItemVm CreateFrom(GetInvoiceProductItemResponse response)
    //{
    //    return new ProductItemVm
    //    {
    //        Id = response.Id,
    //        ExchangeRate = response.ExchangeRate,
    //        GramPrice = response.GramPrice,
    //        ProfitPercent = response.ProfitPercent,
    //        TaxPercent = response.TaxPercent,
    //        CostPrice = response.CostPrice,
    //        CostPriceExchangeRate = response.CostPriceExchangeRate,
    //        CostPriceUnitId = response.CostPriceUnitId,
    //        CostPriceUnitTitle = response.CostPriceUnitTitle,
    //        IsInstantProduct = response.IsInstantProduct,
    //        Product = ProductVm.CreateFrom(response.Product)
    //    };
    //}
}