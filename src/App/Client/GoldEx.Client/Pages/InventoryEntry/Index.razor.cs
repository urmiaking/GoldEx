using GoldEx.Client.Components.Services;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("انبار", href: ClientRoutes.InventoryStocks.Index, icon: Icons.Material.Filled.Warehouse),
        new("ورود دستی", href: ClientRoutes.InventoryStocks.InventoryEntry.Index, icon: Icons.Material.Filled.Add)
    ];

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "manual-entry-video";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }
}