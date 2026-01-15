using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class CurrencyInventoryPageBase : QueryPersistedReportPage<CurrencyInventoryFilterVm>
{
    protected List<CurrencyInventoryRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("موجودی ارز", ClientRoutes.Reporting.CurrencyInventory, icon: Icons.Material.Filled.CurrencyExchange)
    ];

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

        Filters.ItemStatus = itemStatus;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.ItemStatus,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((CurrencyInventoryFilterVm)filter);
    }

    protected async Task LoadReportAsync(CurrencyInventoryFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<CurrencyInventoryRpResponse>>(
            action: (s, ct) => s.GetCurrencyInventoryAsync(request, ct));

        IsLoading = false;
    }
}