using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class UsedProductsList
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? TableClass { get; set; }

    [Parameter]
    public int Elevation { get; set; } = 1;

    [Parameter]
    public List<UsedProductVm> Items { get; set; } = [];

    [Parameter]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Parameter]
    public EventCallback OnAddItem { get; set; }

    [Parameter]
    public EventCallback<UsedProductVm> OnEditItem { get; set; }

    [Parameter]
    public EventCallback<UsedProductVm> OnRemoveItem { get; set; }

    [Parameter]
    public EventCallback<UsedProductVm> OnPrintBarcode { get; set; }
}