using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("اسناد مالی", href: ClientRoutes.Finances.Index, icon: Icons.Material.Filled.LibraryBooks)
    ];
}