using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceItemVm
{
    public Guid? Id { get; set; }

    [Display(Name = "سود")]
    public decimal ProfitPercent { get; set; }

    [Display(Name = "مالیات")]
    public decimal TaxPercent { get; set; }

    [Display(Name = "نرخ هر گرم طلا")]
    public decimal GramPrice { get; set; }

    [Display(Name = "نرخ تبدیل ارز")]
    public decimal? ExchangeRate { get; set; }

    [Display(Name = "تعداد")]
    public int Quantity { get; set; }

    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    public ProductVm Product { get; set; } = ProductVm.CreateDefaultInstance();

    // --- Display properties ---
    public bool ShowDetails { get; set; }
    public int Index { get; set; } = 1;

    // --- Calculated Properties ---
    public decimal RawAmount => CalculatorHelper.CalculateRawPrice(Product.Weight ?? 0, GramPrice, Product.CaratType, Product.ProductType);
    public decimal WageAmount => CalculatorHelper.CalculateWage(RawAmount, Product.Weight ?? 0, Product.Wage, Product.WageType, ExchangeRate);
    public decimal ProfitAmount => CalculatorHelper.CalculateProfit(RawAmount, WageAmount, Product.ProductType, ProfitPercent);
    public decimal TaxAmount => CalculatorHelper.CalculateTax(WageAmount, ProfitAmount, TaxPercent, Product.ProductType);
    public decimal FinalAmount => RawAmount + WageAmount + ProfitAmount + TaxAmount;
    public decimal TotalAmount => FinalAmount * Quantity;

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
}