using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static GoldEx.Shared.Helpers.CalculatorHelper;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class ProductItemVm
{
    public Guid? Id { get; set; }

    private decimal _gramPrice;
    private decimal _profitPercent;
    private decimal _taxPercent;
    private decimal? _wageExchangeRate;
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
    public decimal? WageExchangeRate
    {
        get => _wageExchangeRate;
        set
        {
            _wageExchangeRate = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "نرخ خرید هر واحد جنس")]
    public decimal? CostPrice { get; set; }

    [Display(Name = "واحد ارزی نرخ خرید جنس")]
    public Guid? CostPriceUnitId { get; set; }

    [Display(Name = "واحد ارزی نرخ خرید جنس")]
    public string? CostPriceUnitTitle { get; set; }

    [Display(Name = "نرخ تبدیل واحد ارزش جنس")]
    public decimal? CostPriceExchangeRate { get; set; }

    [Display(Name = "نرخ تبدیل واحد سنگ")]
    public decimal? StonePriceUnitExchangeRate { get; set; }

    public bool IsInstantProduct { get; set; }

    public InvoiceType InvoiceType { get; set; }

    [Display(Name = "تعداد")]
    public int Quantity { get; set; } = 1;

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

    /// <summary>
    /// This method performs the client-side calculation and updates the display properties.
    /// It's called whenever an input property changes.
    /// </summary>
    public ProductItemVm RecalculateAmounts()
    {
        if (InvoiceType is InvoiceType.Purchase)
        {
            RawAmount = (CostPrice * (CostPriceExchangeRate ?? 1) * Quantity) ?? 0;
            WageAmount = 0;
            ProfitAmount = 0;
            TaxAmount = 0;
            FinalAmount = RawAmount;
        }
        else
        {
            RawAmount = CalculatorHelper.Product.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.Fineness, Quantity, Product.ProductType);
            WageAmount = CalculatorHelper.Product.CalculateWage(RawAmount, Product.Weight ?? 0, Product.Wage, Product.WageType, WageExchangeRate);
            ProfitAmount = CalculatorHelper.Product.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
            TaxAmount = CalculatorHelper.Product.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
            FinalAmount = CalculatorHelper.Product.CalculateFinalPrice(RawAmount, WageAmount, ProfitAmount, TaxAmount, 0, Product.ProductType) 
                + (Product.Stones != null ? Product.Stones.Sum(x => x.Cost * (StonePriceUnitExchangeRate ?? 1)) : 0);
        }

        return this;
    }

    public static ProductItemVm CreateDefaultInstance()
    {
        return new ProductItemVm
        {
            Product = ProductVm.CreateDefaultInstance(),
            GramPrice = 0,
            WageExchangeRate = null,
            ProfitPercent = 0,
            TaxPercent = 0,
            Quantity = 1
        };
    }

    public void UpdateFrom(ProductItemVm other)
    {
        Index = other.Index;
        Product = other.Product;
        GramPrice = other.GramPrice;
        WageExchangeRate = other.WageExchangeRate;
        ProfitPercent = other.ProfitPercent;
        TaxPercent = other.TaxPercent;
        CostPrice = other.CostPrice;
        CostPriceExchangeRate = other.CostPriceExchangeRate;
        CostPriceUnitId = other.CostPriceUnitId;
        CostPriceUnitTitle = other.CostPriceUnitTitle;
        IsInstantProduct = other.IsInstantProduct;
        ShowDetails = other.ShowDetails;
        InvoiceType = other.InvoiceType;
        Quantity = other.Quantity;
    }

    public static InvoiceProductItemDto ToRequest(ProductItemVm productItem)
    {
        return new InvoiceProductItemDto(
            productItem.Id,
            productItem.GramPrice,
            productItem.ProfitPercent,
            productItem.TaxPercent,
            productItem.CostPrice,
            productItem.CostPriceExchangeRate,
            productItem.WageExchangeRate,
            productItem.StonePriceUnitExchangeRate,
            productItem.CostPriceUnitId,
            productItem.IsInstantProduct,
            productItem.Quantity,
            ProductVm.ToRequest(productItem.Product));
    }

    public static ProductItemVm CreateFrom(GetInvoiceProductItemResponse response, InvoiceType invoiceType)
    {
        return new ProductItemVm
        {
            Id = response.Id,
            WageExchangeRate = response.WageExchangeRate,
            GramPrice = response.GramPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            CostPrice = response.CostPrice,
            CostPriceExchangeRate = response.CostPriceExchangeRate,
            CostPriceUnitId = response.CostPriceUnitId,
            CostPriceUnitTitle = response.CostPriceUnitTitle,
            StonePriceUnitExchangeRate = response.StonePriceUnitExchangeRate,
            IsInstantProduct = response.IsInstantProduct,
            Product = ProductVm.CreateFromInvoice(response),
            Quantity = response.Quantity,
            InvoiceType = invoiceType
        };
    }
}