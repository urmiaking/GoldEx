using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Statics;

public partial class About
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("درباره ما", href: ClientRoutes.About.Index, icon: Icons.Material.Filled.Info)
    ];
}