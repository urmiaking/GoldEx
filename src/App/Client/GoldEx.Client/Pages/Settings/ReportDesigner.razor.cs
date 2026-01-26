using GoldEx.Client.Components.Services;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class ReportDesigner
{
    [Parameter] public string Name { get; set; } = default!;
    [Parameter, SupplyParameterFromQuery] public string? DisplayName { get; set; }
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new ("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new ("گزارش ها", href: ClientRoutes.Settings.ReportsList, icon: Icons.Material.Filled.DesignServices),
        new ("طراحی گزارش", href: null, icon: Icons.Material.Filled.Edit),
    ];

    protected override void OnInitialized()
    {
        HelpContext.Slug = "report-list";
        base.OnInitialized();
    }
}