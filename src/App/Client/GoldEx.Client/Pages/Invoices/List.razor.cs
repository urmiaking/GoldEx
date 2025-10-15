using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class List
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("فاکتور ها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.LibraryBooks),
        new("لیست", href: null, icon: Icons.Material.Filled.List)
    ];
}
