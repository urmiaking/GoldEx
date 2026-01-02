using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting;


public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("گزارش ها", href: ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart)
    ];
}
