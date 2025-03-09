using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("محاسبه گر", href: ClientRoutes.Calculator.Index, icon: Icons.Material.Outlined.Calculate)
    ];
}