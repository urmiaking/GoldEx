using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Helpers;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class CoinItemVm
{
    public Guid? Id { get; set; }

    private int _quantity;
    private decimal _unitPrice;
    private decimal _profitPercent;

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

    [Display(Name = "سکه")]
    public CoinInstanceVm CoinInstance { get; set; } = new ();

    public bool IsInstant { get; set; }

    // --- Display properties ---
    public bool ShowDetails { get; set; }
    public int Index { get; set; } = 1;

    // --- Calculated Properties ---
    public decimal RawAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// This method performs the client-side calculation and updates the display properties.
    /// It's called whenever an input property changes.
    /// </summary>
    public void RecalculateAmounts()
    {
        RawAmount = UnitPrice;
        ProfitAmount = CalculatorHelper.Coin.CalculateProfit(UnitPrice, ProfitPercent, Quantity);
        FinalAmount = RawAmount + ProfitAmount;
        TotalAmount = FinalAmount * Quantity;
    }

    public Task<CoinItemVm> RecalculateAmountsAsync()
    {
        RawAmount = UnitPrice;
        ProfitAmount = CalculatorHelper.Coin.CalculateProfit(UnitPrice, ProfitPercent, Quantity);
        FinalAmount = RawAmount + ProfitAmount;
        TotalAmount = FinalAmount * Quantity;

        return Task.FromResult(this);
    }

    public static InvoiceCoinItemDto ToRequest(CoinItemVm coinItem)
    {
        if (coinItem.CoinInstance is null)
            throw new FluentValidation.ValidationException("سکه انتخاب نشده است");

        return new InvoiceCoinItemDto(coinItem.Id,
            coinItem.UnitPrice,
            coinItem.Quantity,
            coinItem.ProfitPercent,
            coinItem.IsInstant,
            coinItem.CoinInstance.ToRequest());
    }

    public static CreateCoinItemEntryRequest ToInventoryEntryRequest(CoinItemVm coinItem)
    {
        if (coinItem.CoinInstance is null)
            throw new FluentValidation.ValidationException("سکه انتخاب نشده است");

        return new CreateCoinItemEntryRequest( coinItem.Quantity, coinItem.UnitPrice, coinItem.CoinInstance.ToRequest());
    }

    public static CoinItemVm CreateFrom(GetInvoiceCoinItemResponse response)
    {
        return new CoinItemVm
        {
            Id = response.Id,
            UnitPrice = response.UnitPrice,
            ProfitPercent = response.ProfitPercent,
            Quantity = response.Quantity,
            IsInstant = response.IsInstant,
            CoinInstance = CoinInstanceVm.CreateFrom(response.Coin)
        };
    }

    public void UpdateFrom(CoinItemVm coinItem)
    {
        Index = coinItem.Index;
        UnitPrice = coinItem.UnitPrice;
        ProfitPercent = coinItem.ProfitPercent;
        Quantity = coinItem.Quantity;
        IsInstant = coinItem.IsInstant;
        CoinInstance = coinItem.CoinInstance;
        ShowDetails = coinItem.ShowDetails;
    }
}