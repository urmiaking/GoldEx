using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("انبار", href: ClientRoutes.InventoryStocks.Index, icon: Icons.Material.Filled.Warehouse),
        new("ورود دستی اجناس", href: ClientRoutes.InventoryStocks.InventoryEntry.Index, icon: Icons.Material.Filled.Add)
    ];
}