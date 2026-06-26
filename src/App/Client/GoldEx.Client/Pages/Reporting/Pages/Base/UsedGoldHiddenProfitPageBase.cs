using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class UsedGoldHiddenProfitPageBase : QueryPersistedReportPage<UsedGoldHiddenProfitFilterVm>
{
    protected List<UsedGoldHiddenProfitRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("سود پنهان طلای مستعمل", ClientRoutes.Reporting.UsedGoldHiddenProfit, icon: Icons.Material.Filled.TrendingUp)
    ];

    [SupplyParameterFromQuery(Name = "customerId")]
    public Guid? CustomerId { get; set; }

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        Filters.CustomerId = CustomerId;
        Filters.PriceUnitId = PriceUnitId;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.CustomerId,
            Filters.PriceUnitId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((UsedGoldHiddenProfitFilterVm)filter);
    }

    protected async Task LoadReportAsync(UsedGoldHiddenProfitFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<UsedGoldHiddenProfitRpResponse>>(
            action: (s, ct) => s.GetUsedGoldHiddenProfitAsync(request, ct));

        IsLoading = false;
    }
}
