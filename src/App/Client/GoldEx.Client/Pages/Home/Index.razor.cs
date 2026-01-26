using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace GoldEx.Client.Pages.Home;

public partial class Index
{
    [Inject] public IBrowserViewportService BrowserViewportService { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new ("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];

    private string PriceBoardClass => User?.Identity?.IsAuthenticated ?? false ? "responsive-table" : "responsive-table-login";
}