using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ProductItemsList
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public int Elevation { get; set; } = 1;

    [Parameter]
    public List<ProductItemVm> Items { get; set; } = [];

    [Parameter]
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Parameter]
    public InvoiceType InvoiceType { get; set; }

    [Parameter]
    public EventCallback OnAddItem { get; set; }

    [Parameter]
    public EventCallback OnOpenSelector { get; set; }

    [Parameter]
    public EventCallback<ProductItemVm> OnEditItem { get; set; }

    [Parameter]
    public EventCallback<ProductItemVm> OnRemoveItem { get; set; }

    [Parameter]
    public EventCallback<string> OnBarcodeChanged { get; set; }

    [Parameter] public EventCallback<ProductItemVm> OnPrintBarcode { get; set; }

    private string? _barcode;

    private Task OnBarcodeCleared()
    {
        _barcode = null;
        return OnBarcodeChanged.InvokeAsync(null);
    }
}