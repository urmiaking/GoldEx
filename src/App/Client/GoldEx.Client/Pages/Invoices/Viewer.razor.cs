using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class Viewer
{
    [Parameter] public long Number { get; set; }

    private string? _reportUrl;
    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnInitialized()
    {
        if (Number != 0) _reportUrl = $"InvoiceReport?invoiceNumber={Number}";
    }

    protected override void OnParametersSet()
    {
        _breadcrumbs =
        [
            new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("فاکتورها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.ReceiptLong),
            new BreadcrumbItem("مشاهده فاکتور", href: null, icon: Icons.Material.Filled.ViewTimeline)
        ];
    }
}