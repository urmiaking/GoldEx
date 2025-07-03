using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class SetInvoice
{
    [Parameter] public string? Id { get; set; }

    private Guid? IdValue => string.IsNullOrEmpty(Id) ? null : Guid.Parse(Id);

    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnParametersSet()
    {
        _breadcrumbs =
        [
            new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
            new("فاکتورها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.ReceiptLong),
            new(Id == null ? "فاکتور جدید" : "ویرایش فاکتور", href: null, icon: Icons.Material.Filled.Edit)
        ];
    }

}