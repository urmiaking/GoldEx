using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions;

public partial class Create
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("ثبت تراکنش جدید", href: ClientRoutes.Transactions.Create, icon: Icons.Material.Filled.PostAdd)
    ];
}