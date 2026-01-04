using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class LedgerAccountDetailsPageBase
    : QueryPersistedReportPage<LedgerAccountDetailsFilterVm>
{
    protected List<LedgerAccountStatementRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("گردش حساب", ClientRoutes.Reporting.LedgerAccountDetails, icon: Icons.Material.Filled.AccountBalanceWallet)
    ];

    /* ---------------- Query parameters ---------------- */

    [SupplyParameterFromQuery(Name = "ledgerAccountId")]
    public Guid? LedgerAccountId { get; set; }

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        Filters.LedgerAccountId = LedgerAccountId;
        Filters.PriceUnitId = PriceUnitId;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.LedgerAccountId,
            Filters.PriceUnitId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((LedgerAccountDetailsFilterVm)filter);
    }

    protected async Task LoadReportAsync(LedgerAccountDetailsFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<LedgerAccountStatementRpResponse>>(
            action: (s, ct) => s.GetLedgerAccountStatementsAsync(request, ct));

        IsLoading = false;
    }
}
