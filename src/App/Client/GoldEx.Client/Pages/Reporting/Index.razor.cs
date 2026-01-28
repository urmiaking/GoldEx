using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("گزارش ها", href: ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart)
    ];

    private GetLicenseResponse? _license;

    protected override async Task OnInitializedAsync()
    {
        await LoadLicenseAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadLicenseAsync()
    {
        await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            afterSend: response => _license = response);
    }
}
