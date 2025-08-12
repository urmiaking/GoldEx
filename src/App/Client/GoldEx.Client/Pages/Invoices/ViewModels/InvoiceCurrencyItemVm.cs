using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceCurrencyItemVm
{
    private int _amount;
    private decimal _unitPrice;
    private decimal _taxPercent;
    private decimal _profitPercent;

    [Display(Name = "حجم")]
    public int Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            RecalculateAmounts();
        }
    }

    [Display(Name = "قیمت واحد")]
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            _unitPrice = value;
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

    [Display(Name = "نوع ارز")]
    public PriceUnitVm? Currency { get; set; }

    // --- Display properties ---
    public bool ShowDetails { get; set; }
    public int Index { get; set; } = 1;

    // --- Calculated Properties ---
    public decimal RawAmount { get; set; }
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
        RawAmount = UnitPrice;
        ProfitAmount = CalculatorHelper.Currency.CalculateProfit(UnitPrice, ProfitPercent);
        TaxAmount = CalculatorHelper.Currency.CalculateTax(UnitPrice, TaxPercent);
        FinalAmount = RawAmount + ProfitAmount + TaxAmount;
        TotalAmount = FinalAmount * Amount;
    }

    public static InvoiceCurrencyItemDto ToRequest(InvoiceCurrencyItemVm currencyItem)
    {
        return new InvoiceCurrencyItemDto(currencyItem.UnitPrice, currencyItem.Amount, currencyItem.ProfitPercent, currencyItem.TaxPercent, currencyItem.Currency);
    }

    public static InvoiceCurrencyItemDto CreateFrom(InvoiceCurrencyItemDto response)
    {
        return new InvoiceCurrencyItemDto
        {
            UnitPrice = response.UnitPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            Amount = response.Amount,
            CurrencyId = ProductVm.CreateFrom(response.)
        };
    }
}