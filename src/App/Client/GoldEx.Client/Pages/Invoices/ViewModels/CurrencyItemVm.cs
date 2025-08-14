using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class CurrencyItemVm
{
    public Guid? Id { get; set; }

    private decimal _amount;
    private decimal _unitPrice;
    private decimal _taxPercent;
    private decimal _profitPercent;

    [Display(Name = "حجم")]
    public decimal Amount
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
    public GetPriceUnitTitleResponse Currency { get; set; } = default!;

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
        ProfitAmount = CalculatorHelper.Currency.CalculateProfit(UnitPrice, Amount, ProfitPercent);
        TaxAmount = CalculatorHelper.Currency.CalculateTax(UnitPrice, Amount, TaxPercent);
        FinalAmount = RawAmount + ProfitAmount + TaxAmount;
        TotalAmount = FinalAmount * Amount;
    }

    public static InvoiceCurrencyItemDto ToRequest(CurrencyItemVm currencyItem)
    {
        if (currencyItem.Currency is null)
            throw new FluentValidation.ValidationException("ارز وارد نشده است");

        return new InvoiceCurrencyItemDto(currencyItem.Id,
            currencyItem.UnitPrice,
            currencyItem.Amount,
            currencyItem.ProfitPercent,
            currencyItem.TaxPercent,
            currencyItem.Currency.Id);
    }

    public static CurrencyItemVm CreateFrom(GetInvoiceCurrencyItemResponse response)
    {
        return new CurrencyItemVm
        {
            Id = response.Id,
            UnitPrice = response.UnitPrice,
            ProfitPercent = response.ProfitPercent,
            TaxPercent = response.TaxPercent,
            Amount = response.Amount,
            Currency = response.Currency
        };
    }

    public void UpdateFrom(CurrencyItemVm coinItem)
    {
        Index = coinItem.Index;
        UnitPrice = coinItem.UnitPrice;
        ProfitPercent = coinItem.ProfitPercent;
        Amount = coinItem.Amount;
        Currency = coinItem.Currency;
        TaxPercent = coinItem.TaxPercent;
        ShowDetails = coinItem.ShowDetails;
    }
}