using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("لیست مشتریان", href: ClientRoutes.Customers.Index, icon: Icons.Material.Filled.PeopleAlt)
    ];
}