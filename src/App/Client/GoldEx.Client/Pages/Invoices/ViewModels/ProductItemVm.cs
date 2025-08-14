using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class ProductItemVm
{
    public Guid? Id { get; set; }

    private decimal _gramPrice;
    private decimal _profitPercent;
    private decimal _taxPercent;
    private decimal? _exchangeRate;
    private ProductVm _product = ProductVm.CreateDefaultInstance();

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
            _product.PropertyChanged -= OnProductChanged;

            _product = value;

            _product.PropertyChanged += OnProductChanged;

            RecalculateAmounts();
        }
    }

    private void OnProductChanged(object? sender, PropertyChangedEventArgs e)
    {
        RecalculateAmounts();
    }

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
        RawAmount = CalculatorHelper.Product.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.CaratType, Product.ProductType);
        WageAmount = CalculatorHelper.Product.CalculateWage(RawAmount, Product.Weight ?? 0, Product.Wage, Product.WageType, ExchangeRate);
        ProfitAmount = CalculatorHelper.Product.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
        TaxAmount = CalculatorHelper.Product.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
        FinalAmount = RawAmount + WageAmount + ProfitAmount + TaxAmount;
        TotalAmount = FinalAmount;
    }

    public static ProductItemVm CreateDefaultInstance()
    {
        return new ProductItemVm
        {
            Product = ProductVm.CreateDefaultInstance(),
            GramPrice = 0,
            ExchangeRate = null,
            ProfitPercent = 0,
            TaxPercent = 0
        };
    }

    public void UpdateFrom(ProductItemVm other)
    {
        Index = other.Index;
        Product = other.Product;
        GramPrice = other.GramPrice;
        ExchangeRate = other.ExchangeRate;
        ProfitPercent = other.ProfitPercent;
        TaxPercent = other.TaxPercent;
        ShowDetails = other.ShowDetails;
    }

    public static InvoiceProductItemDto ToRequest(ProductItemVm productItem)
    {
        return new InvoiceProductItemDto(
            productItem.Id,
            productItem.GramPrice,
            productItem.ProfitPercent,
            productItem.TaxPercent,
            productItem.ExchangeRate,
            ProductVm.ToRequest(productItem.Product));
    }

    public static ProductItemVm CreateFrom(GetInvoiceProductItemResponse response)
    {
        return new ProductItemVm
        {
            Id = response.Id,
            ExchangeRate = response.ExchangeRate,
            GramPrice = response.GramPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            Product = ProductVm.CreateFrom(response.Product)
        };
    }
}