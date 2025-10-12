using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.InventoryStocks.Components.MeltingBatches;

public partial class MeltedGoldStep
{
    [Parameter] public MeltingBatchVm Model { get; set; } = new();
    [Parameter] public EventCallback<MeltingBatchVm> ModelChanged { get; set; }
    [Parameter] public bool Processing { get; set; }
    [Parameter] public EventCallback OnCompleteProcess { get; set; }
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public List<GetFinancialAccountTitleResponse> FinancialAccounts { get; set; } = [];
    [Parameter] public EventCallback OnAddFinancialAccount { get; set; }
    [Parameter] public EventCallback<GetPriceUnitTitleResponse> OnSelectFeePriceUnit { get; set; }

    private bool _feeFieldMenuOpen;

    private bool IsCompleteProcessDisabled =>
        Processing ||
        Model.MeltedGoldWeight is null ||
        Model.Fineness is null ||
        string.IsNullOrEmpty(Model.AssayNumber) ||
        Model.GramPrice is null ||
        Model is { FeeAmount: not null, FinancialAccount: null };

    private string? FeeExchangeRateLabel =>
        Model is { FeePriceUnit: not null, PriceUnit: not null }
            ? $"نرخ تبدیل {Model.FeePriceUnit.Title} به {Model.PriceUnit.Title}"
            : null;

    private int GetDescriptionMd()
    {
        var used = 3; // FeeAmount always visible
        if (Model.FeePriceUnit != Model.PriceUnit)
            used += 3; // Exchange rate field
        if (Model.FeeAmount.HasValue)
            used += 3; // Financial account field
        return 12 - used; // Remaining for description
    }
}