using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class Viewer
{
    [Parameter] public Guid Id { get; set; }

    private string? _reportUrl;
    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnInitialized()
    {
        if (Id != Guid.Empty) _reportUrl = $"InvoiceReport?id={Id}";
    }

    protected override void OnParametersSet()
    {
        _breadcrumbs =
        [
            new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
            new("فاکتورها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.ReceiptLong),
            new("مشاهده فاکتور", href: null, icon: Icons.Material.Filled.ViewTimeline)
        ];
    }
}