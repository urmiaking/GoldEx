using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CoinItemsList
{
    [Parameter]
    public string? Class { get; set; }

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
    public EventCallback<CoinItemVm> OnRemoveItem { get; set; }
}