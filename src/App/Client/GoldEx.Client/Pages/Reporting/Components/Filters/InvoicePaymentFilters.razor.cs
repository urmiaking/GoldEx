using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class InvoicePaymentFilters
{
    [Parameter] public InvoicePaymentFilterVm Model { get; set; } = default!;

    private Color GetColor(InvoiceType item)
    {
        return item switch
        {
            InvoiceType.Purchase => Color.Success,
            InvoiceType.Sell => Color.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }

    private string GetIcon(InvoiceType item)
    {
        return item switch
        {
            InvoiceType.Purchase => Icons.Material.Outlined.ArrowDownward,
            InvoiceType.Sell => Icons.Material.Outlined.ArrowUpward,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}