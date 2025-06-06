using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceItemVm
{
    public Guid? Id { get; set; }

    [Display(Name = "سود")]
    public decimal ProfitPercent { get; private set; }

    [Display(Name = "مالیات")]
    public decimal TaxPercent { get; private set; }

    [Display(Name = "نرخ گرم 18 عیار")]
    public decimal GramPrice { get; private set; }

    [Display(Name = "نرخ تبدیل ارز")]
    public decimal? ExchangeRate { get; private set; }

    [Display(Name = "تعداد")]
    public int Quantity { get; private set; }

    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    public ProductVm Product { get; set; } = ProductVm.CreateDefaultInstance();

    // --- Calculated Properties ---
    public decimal RawAmount => CalculatorHelper.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.CaratType, Product.ProductType);
    public decimal WageAmount => CalculatorHelper.CalculateWage(RawAmount, Product.Wage, Product.WageType, ExchangeRate);
    public decimal ProfitAmount => CalculatorHelper.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
    public decimal TaxAmount => CalculatorHelper.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
    public decimal FinalAmount => RawAmount + WageAmount + ProfitAmount + TaxAmount;
    public decimal TotalAmount => FinalAmount * Quantity;
}