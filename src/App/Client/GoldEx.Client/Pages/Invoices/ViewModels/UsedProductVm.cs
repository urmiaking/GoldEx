using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class UsedProductVm
{
    private decimal _gramPrice;
    private decimal? _weight;
    private decimal? _exchangeRate;
    private decimal? _finenessDeductionRate = 15;

    public Guid? Id { get; set; }

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
    public decimal Fineness { get; set; }

    [Display(Name = "کسری عیار")]
    [Range(-250, 750, ErrorMessage = "{0} باید بین {1} و {2} باشد.")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal? FinenessDeductionRate
    {
        get => _finenessDeductionRate;
        set
        {
            _finenessDeductionRate = value;
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

    [Display(Name = "هزینه های جانبی")]
    public decimal? ExtraCostsAmount { get; set; }

    [Display(Name = "آیا طلای شکسته است؟")]
    public bool IsBroken { get; set; }

    [Display(Name = "مبلغ کالا")]
    public decimal ItemAmount { get; set; }

    [Display(Name = "ارزش کل")]
    public decimal ItemFinalAmount => ItemAmount + (ExtraCostsAmount ?? 0);

    [Display(Name = "نوع واحد طلا")]
    public GoldUnitType UnitType { get; set; }

    public int Index { get; set; } = 1;

    /// <summary>
    /// This method performs the client-side calculation and updates the display properties.
    /// It's called whenever an input property changes.
    /// </summary>
    public void RecalculateAmounts()
    {
        if (Weight == null || FinenessDeductionRate == null || GramPrice <= 0)
        {
            ItemAmount = 0;
            return;
        }

        ItemAmount = CalculatorHelper.UsedProduct.Calculate(Weight.Value, 0, FinenessDeductionRate.Value, GramPrice, 1, ExchangeRate) + (ExtraCostsAmount ?? 0);
    }

    public void UpdateFrom(UsedProductVm other)
    {
        Id = other.Id;
        Index = other.Index;
        GramPrice = other.GramPrice;
        ExchangeRate = other.ExchangeRate;
        Fineness = other.Fineness;
        Weight = other.Weight;
        Description = other.Description;
        ExtraCostsAmount = other.ExtraCostsAmount;
        UnitType = other.UnitType;
        IsBroken = other.IsBroken;
    }

    public static InvoiceUsedProductDto ToRequest(UsedProductVm productItem)
    {
        return new InvoiceUsedProductDto(productItem.Id,
            productItem.Description ?? string.Empty,
            productItem.Weight ?? 0,
            productItem.GramPrice,
            productItem.ExtraCostsAmount ?? 0,
            productItem.FinenessDeductionRate ?? 0,
            1,
            productItem.IsBroken,
            ProductType.UsedGold,
            productItem.UnitType);
    }

    public static UsedProductVm CreateFrom(GetInvoiceUsedProductResponse response)
    {
        return new UsedProductVm
        {
            Id = response.Id,
            GramPrice = response.GramPrice,
            FinenessDeductionRate = response.FinenessDeductionRate,
            Weight = response.Weight,
            Description = response.Description,
            ExtraCostsAmount = response.ExtraCostsAmount,
            ItemAmount = response.ItemAmount,
            UnitType = response.UnitType,
            IsBroken = response.IsBroken,
        };
    }
}