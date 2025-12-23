using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CoinItemsList
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? TableClass { get; set; }

    [Parameter]
    public int Elevation { get; set; } = 1;

    [Parameter]
    public List<CoinItemVm> Items { get; set; } = [];

    [Parameter]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Parameter]
    public EventCallback OnAddItem { get; set; }

    [Parameter]
    public InvoiceType InvoiceType { get; set; }

    [Parameter]
    public EventCallback OnOpenSelector { get; set; }

    [Parameter]
    public EventCallback<CoinItemVm> OnEditItem { get; set; }

    [Parameter]
    public EventCallback<CoinItemVm> OnPrintBarcode { get; set; }

    [Parameter]
    public EventCallback<CoinItemVm> OnRemoveItem { get; set; }

    [Parameter]
    public EventCallback<string> OnBarcodeChanged { get; set; }

    private string ListTitle =>
        InvoiceType is InvoiceType.Sell
            ? "فهرست سکه های فروخته‌شده"
            : "فهرست سکه های خریداری‌شده";

    private string? _barcode;

    private Task OnBarcodeCleared()
    {
        _barcode = null;
        return OnBarcodeChanged.InvokeAsync(null);
    }

    private string GetWeight(CoinItemVm context)
    {
        var weight = context.CoinInstance.Weight?.ToWeightFormat(GoldUnitType.Gram);

        var vacuumedWeight = context.CoinInstance.CoinPackage?.VacuumedWeight?.ToWeightFormat(GoldUnitType.Gram);

        return vacuumedWeight is not null
            ? $"{weight} ({vacuumedWeight} با وکیوم)"
            : weight ?? "-";
    }
}