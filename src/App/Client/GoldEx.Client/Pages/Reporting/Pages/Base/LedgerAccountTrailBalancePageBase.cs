using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public class LedgerAccountTrailBalancePageBase
: QueryPersistedReportPage<LedgerAccountTrialBalanceFilterVm>
{
    protected List<LedgerAccountTrialBalanceRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("تراز کلی حساب ها", ClientRoutes.Reporting.LedgerAccountTrialBalances, icon: Icons.Material.Filled.Balance)
    ];

    /* ---------------- Query parameters ---------------- */

    [SupplyParameterFromQuery(Name = "parentLedgerId")]
    public Guid? ParentLedgerId { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        Filters.ParentLedgerId = ParentLedgerId;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.ParentLedgerId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((LedgerAccountTrialBalanceFilterVm)filter);
    }

    protected async Task LoadReportAsync(LedgerAccountTrialBalanceFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<LedgerAccountTrialBalanceRpResponse>>(
            (s, ct) => s.GetLedgerAccountTrialBalanceAsync(request, ct));

        IsLoading = false;
    }
}
