using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers;

public partial class List
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("اسناد مالی", href: ClientRoutes.Finances.Index, icon: Icons.Material.Filled.LibraryBooks),
        new("حواله‌ها بین مشتریان", href: ClientRoutes.Finances.CustomerTransfers, icon: Icons.Material.Filled.SwapHoriz)
    ];

    [Parameter, SupplyParameterFromQuery(Name = "q")] public string? SearchQuery { get; set; }
}
