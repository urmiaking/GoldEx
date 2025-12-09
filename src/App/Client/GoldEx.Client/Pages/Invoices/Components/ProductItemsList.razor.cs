using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ProductItemsList
{
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? TableClass { get; set; }

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

    private string ListTitle =>
        InvoiceType is InvoiceType.Sell
            ? "فهرست اقلام فروخته‌شده"
            : "فهرست اقلام خریداری‌شده";

    private Task OnBarcodeCleared()
    {
        _barcode = null;
        return OnBarcodeChanged.InvokeAsync(null);
    }

    private string GetProductDisplayName(ProductVm product)
    {
        if (!string.IsNullOrWhiteSpace(product.Name))
            return product.Name;

        if (product.ProductType != ProductType.MoltenGold)
            return "-";

        var molten = product.MoltenGold;
        if (molten == null)
            return "-";

        var assayNumber = molten.AssayNumber;
        var assayerName = molten.Assayer?.FullName;
        var fineness = product.Fineness;

        if (!string.IsNullOrWhiteSpace(assayerName) &&
            !string.IsNullOrWhiteSpace(assayNumber))
        {
            return $"آبشده ({assayNumber}) - {assayerName}";
        }

        return !string.IsNullOrWhiteSpace(assayNumber) ? $"آبشده ({assayNumber})" : $"طلای آبشده عیار {fineness:G29}";
    }
}