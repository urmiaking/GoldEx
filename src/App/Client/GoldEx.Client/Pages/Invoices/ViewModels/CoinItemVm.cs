using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
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
    public GetCoinResponse? Coin { get; set; } = default!;

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "وزن الزامی است")]
    public decimal? Weight { get; set; }

    [Display(Name = "عیار")]
    [Required(ErrorMessage = "عیار الزامی است")]
    public decimal Fineness { get; set; } = 900m;

    [Display(Name = "نوع سکه")]
    public CoinMintType CoinMintType { get; set; } = CoinMintType.Banking;

    [Display(Name = "نوع بسته بندی")]
    public CoinPackageType CoinPackageType { get; set; } = CoinPackageType.VacuumSealed;

    [Display(Name = "سال ضرب")]
    public DateTime? MintYear { get; set; }

    [Display(Name = "بسته بندی سکه")]
    public CoinPackageSpecVm? CoinPackage { get; set; } = new();

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
        if (coinItem.Coin is null)
            throw new FluentValidation.ValidationException("سکه انتخاب نشده است");

        return new InvoiceCoinItemDto(coinItem.Id,
            coinItem.UnitPrice,
            coinItem.Quantity,
            coinItem.ProfitPercent,
            coinItem.Coin.Id);
    }

    public static CreateCoinItemEntryRequest ToInventoryEntryRequest(CoinItemVm coinItem)
    {
        if (coinItem.Coin is null)
            throw new FluentValidation.ValidationException("سکه انتخاب نشده است");

        return new CreateCoinItemEntryRequest(coinItem.Coin.Id, coinItem.Quantity, coinItem.UnitPrice);
    }

    public static CoinItemVm CreateFrom(GetInvoiceCoinItemResponse response)
    {
        return new CoinItemVm
        {
            Id = response.Id,
            UnitPrice = response.UnitPrice,
            ProfitPercent = response.ProfitPercent,
            Quantity = response.Quantity,
            Coin = response.Coin
        };
    }

    public void UpdateFrom(CoinItemVm coinItem)
    {
        Index = coinItem.Index;
        UnitPrice = coinItem.UnitPrice;
        ProfitPercent = coinItem.ProfitPercent;
        Quantity = coinItem.Quantity;
        Coin = coinItem.Coin;
        ShowDetails = coinItem.ShowDetails;
    }
}