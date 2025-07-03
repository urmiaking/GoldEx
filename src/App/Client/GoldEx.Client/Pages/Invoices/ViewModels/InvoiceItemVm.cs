using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceItemVm
{

    private int _quantity;
    private decimal _gramPrice;
    private decimal _profitPercent;
    private decimal _taxPercent;
    private decimal? _exchangeRate;
    private ProductVm _product = ProductVm.CreateDefaultInstance();

    public Guid? Id { get; set; }

    [Display(Name = "تعداد")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "نرخ هر گرم طلا")]
    public decimal GramPrice
    {
        get => _gramPrice;
        set
        {
            _gramPrice = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "سود")]
    public decimal ProfitPercent
    {
        get => _profitPercent;
        set
        {
            _profitPercent = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "مالیات")]
    public decimal TaxPercent
    {
        get => _taxPercent;
        set
        {
            _taxPercent = value;
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

    public ProductVm Product
    {
        get => _product;
        set
        {
            _product = value;
            RecalculateAmounts();
        }
    }

    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    // --- Display properties ---
    public bool ShowDetails { get; set; }
    public int Index { get; set; } = 1;

    // --- Calculated Properties ---
    public decimal RawAmount { get; set; }
    public decimal WageAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// This method performs the client-side calculation and updates the display properties.
    /// It's called whenever an input property changes.
    /// </summary>
    public void RecalculateAmounts()
    {
        RawAmount = CalculatorHelper.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.CaratType, Product.ProductType);
        WageAmount = CalculatorHelper.CalculateWage(RawAmount, Product.Weight ?? 0, Product.Wage, Product.WageType, ExchangeRate);
        ProfitAmount = CalculatorHelper.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
        TaxAmount = CalculatorHelper.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
        FinalAmount = RawAmount + WageAmount + ProfitAmount + TaxAmount;
        TotalAmount = FinalAmount * Quantity;
    }

    public static InvoiceItemVm CreateDefaultInstance()
    {
        return new InvoiceItemVm
        {
            Product = ProductVm.CreateDefaultInstance(),
            GramPrice = 0,
            ExchangeRate = null,
            ProfitPercent = 0,
            TaxPercent = 0,
            Quantity = 1
        };
    }

    public InvoiceItemVm Copy(InvoiceItemVm item)
    {
        return new InvoiceItemVm
        {
            Id = item.Id,
            Product = item.Product,
            GramPrice = item.GramPrice,
            ExchangeRate = item.ExchangeRate,
            ProfitPercent = item.ProfitPercent,
            TaxPercent = item.TaxPercent,
            Quantity = item.Quantity,
            PriceUnit = item.PriceUnit,
            Index = item.Index,
            ShowDetails = item.ShowDetails
        };
    }

    public static InvoiceItemDto ToRequest(InvoiceItemVm item)
    {
        if (item.PriceUnit is null)
            throw new FluentValidation.ValidationException($"واحد ارزی جنس {item.Product.Name} وارد نشده است");

        return new InvoiceItemDto(item.Id,
            item.GramPrice,
            item.ProfitPercent,
            item.TaxPercent,
            item.ExchangeRate,
            item.Quantity,
            ProductVm.ToRequest(item.Product),
            item.PriceUnit.Id);
    }

    public static InvoiceItemVm CreateFrom(GetInvoiceItemResponse response)
    {
        return new InvoiceItemVm
        {
            ExchangeRate = response.ExchangeRate,
            GramPrice = response.GramPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            Quantity = response.Quantity,
            Id = response.Id,
            PriceUnit = response.PriceUnit,
            Product = ProductVm.CreateFrom(response.Product)
        };
    }
}