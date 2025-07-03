using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reports;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class ReportsList
{
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    private List<GetReportResponse> _reports = [];
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        await LoadReportsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadReportsAsync()
    {
        _processing = true;

        await SendRequestAsync<IReportService, List<GetReportResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _reports = response;
                _processing = false;
            });
    }

    private void OnDesignReport(GetReportResponse context)
    {
        NavigationManager.NavigateTo(ClientRoutes.Reporting.DesignReport.FormatRoute(new { name = context.Name }).AppendQueryString(new { context.DisplayName }));
    }
}