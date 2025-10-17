using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class SetInvoice
{
    [Parameter] public string? Id { get; set; }
    [Parameter, SupplyParameterFromQuery] public Guid? CustomerId { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? Barcode { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? TradeScale { get; set; }

    private Guid? IdValue => string.IsNullOrEmpty(Id) ? null : Guid.Parse(Id);
    private TradeScale TradeScaleValue => Enum.TryParse<TradeScale>(TradeScale, out var scale) ? scale : Shared.Enums.TradeScale.Wholesale;

    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnParametersSet()
    {
        _breadcrumbs =
        [
            new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("فاکتور ها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.LibraryBooks),
            new BreadcrumbItem("لیست", href: ClientRoutes.Invoices.List, icon: Icons.Material.Filled.List),
            new BreadcrumbItem(Id == null ? "فاکتور جدید" : "ویرایش فاکتور", href: null, icon: Icons.Material.Filled.Edit)
        ];

        base.OnParametersSet();
    }
}