using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Dashboard;

public partial class Dashboard
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("داشبورد", href: ClientRoutes.Dashboard.Index, icon: Icons.Material.Filled.Dashboard)
    ];
}