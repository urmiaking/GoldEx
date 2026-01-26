using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry;

public partial class List
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("انبار", href: ClientRoutes.InventoryStocks.Index, icon: Icons.Material.Filled.Warehouse),
        new("ورود دستی", href: ClientRoutes.InventoryStocks.InventoryEntry.List, icon: Icons.Material.Filled.History)
    ];

    [Parameter, SupplyParameterFromQuery(Name = "q")] public string? SearchQuery { get; set; }
}