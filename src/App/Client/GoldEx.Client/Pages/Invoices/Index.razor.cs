using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("فاکتورها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.Newspaper)
    ];
}
