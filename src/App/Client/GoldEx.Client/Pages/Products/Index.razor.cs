using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Products;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("اجناس من", href: ClientRoutes.Products.Index, icon: Icons.Material.Filled.Warehouse)
    ];
}