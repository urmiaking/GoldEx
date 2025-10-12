using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks;

public partial class SetMeltingBatch
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("انبار", href: ClientRoutes.InventoryStocks.Index, icon: Icons.Material.Filled.Warehouse),
        new("ذوب طلا", href: ClientRoutes.InventoryStocks.MeltingBatches.Index, icon: Icons.Material.Filled.Whatshot),
        new("فرایند ذوب", href: null, icon: Icons.Material.Filled.Transform)
    ];

    [Parameter] public Guid? Id { get; set; }
}