using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Home;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new ("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];

    private string PriceBoardClass => User?.Identity?.IsAuthenticated ?? false ? "responsive-table" : "responsive-table-login";
}