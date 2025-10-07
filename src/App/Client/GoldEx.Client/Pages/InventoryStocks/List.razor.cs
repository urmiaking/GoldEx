using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks;

public partial class List
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("انبار", href: ClientRoutes.InventoryStocks.Index, icon: Icons.Material.Filled.Warehouse),
        new("موجودی", href: ClientRoutes.InventoryStocks.List, icon: Icons.Material.Filled.Task)
    ];
}