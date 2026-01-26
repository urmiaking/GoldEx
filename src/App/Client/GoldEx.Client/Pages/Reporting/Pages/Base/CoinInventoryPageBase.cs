using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class CoinInventoryPageBase : QueryPersistedReportPage<CoinInventoryFilterVm>
{
    protected List<CoinInventoryRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("موجودی سکه", ClientRoutes.Reporting.CoinInventory, icon: Icons.Material.Filled.MonetizationOn)
    ];

    [SupplyParameterFromQuery(Name = "coinId")]
    public Guid? CoinId { get; set; }

    [SupplyParameterFromQuery(Name = "itemStatus")]
    public string? ItemStatus { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    protected override void ReadQueryToFilter()
    {
        ItemStatus? itemStatus = null;

        if (!string.IsNullOrWhiteSpace(ItemStatus) &&
            Enum.TryParse<ItemStatus>(ItemStatus, ignoreCase: true, out var parsedStatus))
        {
            itemStatus = parsedStatus;
        }

        Filters.CoinId = CoinId;
        Filters.ItemStatus = itemStatus;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.CoinId,
            Filters.ItemStatus,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((CoinInventoryFilterVm)filter);
    }

    protected async Task LoadReportAsync(CoinInventoryFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<CoinInventoryRpResponse>>(
            action: (s, ct) => s.GetCoinInventoryAsync(request, ct));

        IsLoading = false;
    }
}