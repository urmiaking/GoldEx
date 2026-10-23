using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class ProfitAnalysisPageBase : QueryPersistedReportPage<ProfitAnalysisFilterVm>
{
    protected ProfitAnalysisRpResponse? Data;
    private int _loadRevision;
    private string? _loadedQuery;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("تحلیل سود فروش", ClientRoutes.Reporting.ProfitAnalysis, icon: Icons.Material.Filled.Insights)
    ];

    [SupplyParameterFromQuery] public DateTime? Start { get; set; }
    [SupplyParameterFromQuery] public DateTime? End { get; set; }
    [SupplyParameterFromQuery] public Guid? CategoryId { get; set; }
    [SupplyParameterFromQuery] public Guid? CustomerId { get; set; }
    [SupplyParameterFromQuery(Name = "type")] public string? Type { get; set; }

    protected override void ReadQueryToFilter()
    {
        Filters.DateRange = DateRangeExtensions.From(Start, End);
        Filters.CategoryId = CategoryId;
        Filters.CustomerId = CustomerId;
        Filters.ProductType = Enum.TryParse<ProductType>(Type, true, out var type) &&
            type is ProductType.Gold or ProductType.Jewelry ? type : null;
    }

    protected override object WriteFilterToQuery() => new
    {
        Filters.DateRange?.Start, Filters.DateRange?.End, Filters.CategoryId, Filters.CustomerId, Type = Filters.ProductType
    };

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedQuery != Navigation.Uri)
        {
            _loadedQuery = Navigation.Uri;
            await LoadReportAsync();
        }
        await base.OnParametersSetAsync();
    }

    protected Task OnSubmit(ReportFilterVmBase _)
    {
        var previousUri = Navigation.Uri;
        PersistFiltersToQuery();
        return Navigation.Uri == previousUri ? LoadReportAsync() : Task.CompletedTask;
    }

    protected async Task LoadReportAsync()
    {
        var revision = ++_loadRevision;
        IsLoading = true;
        try
        {
            var request = Filters.ToRequest();
            var report = await SendRequestAsync<IReportingService, ProfitAnalysisRpResponse>(
                (s, ct) => s.GetProfitAnalysisAsync(request, ct), createScope: true);
            if (revision == _loadRevision) Data = report;
        }
        finally { if (revision == _loadRevision) IsLoading = false; }
    }
}
