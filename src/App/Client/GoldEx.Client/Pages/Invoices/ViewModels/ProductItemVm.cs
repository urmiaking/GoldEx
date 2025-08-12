using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class ProductItemVm
{
    private int _quantity;
    private decimal _gramPrice;
    private decimal _profitPercent;
    private decimal _taxPercent;
    private decimal? _exchangeRate;
    private ProductVm _product = ProductVm.CreateDefaultInstance();

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
        RawAmount = CalculatorHelper.Product.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.CaratType, Product.ProductType);
        WageAmount = CalculatorHelper.Product.CalculateWage(RawAmount, Product.Weight ?? 0, Product.Wage, Product.WageType, ExchangeRate);
        ProfitAmount = CalculatorHelper.Product.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
        TaxAmount = CalculatorHelper.Product.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
        FinalAmount = RawAmount + WageAmount + ProfitAmount + TaxAmount;
        TotalAmount = FinalAmount * Quantity;
    }

    public static ProductItemVm CreateDefaultInstance()
    {
        return new ProductItemVm
        {
            Product = ProductVm.CreateDefaultInstance(),
            GramPrice = 0,
            ExchangeRate = null,
            ProfitPercent = 0,
            TaxPercent = 0,
            Quantity = 1
        };
    }

    public ProductItemVm Copy(ProductItemVm productItem)
    {
        return new ProductItemVm
        {
            Product = productItem.Product,
            GramPrice = productItem.GramPrice,
            ExchangeRate = productItem.ExchangeRate,
            ProfitPercent = productItem.ProfitPercent,
            TaxPercent = productItem.TaxPercent,
            Quantity = productItem.Quantity,
            PriceUnit = productItem.PriceUnit,
            Index = productItem.Index,
            ShowDetails = productItem.ShowDetails
        };
    }

    public static InvoiceProductItemDto ToRequest(ProductItemVm productItem)
    {
        if (productItem.PriceUnit is null)
            throw new FluentValidation.ValidationException($"واحد ارزی جنس {productItem.Product.Name} وارد نشده است");

        return new InvoiceProductItemDto(
            productItem.GramPrice,
            productItem.ProfitPercent,
            productItem.TaxPercent,
            productItem.ExchangeRate,
            productItem.Quantity,
            ProductVm.ToRequest(productItem.Product),
            productItem.PriceUnit.Id);
    }

    public static ProductItemVm CreateFrom(GetInvoiceProductItemResponse response)
    {
        return new ProductItemVm
        {
            ExchangeRate = response.ExchangeRate,
            GramPrice = response.GramPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            Quantity = response.Quantity,
            PriceUnit = response.PriceUnit,
            Product = ProductVm.CreateFrom(response.Product)
        };
    }
}