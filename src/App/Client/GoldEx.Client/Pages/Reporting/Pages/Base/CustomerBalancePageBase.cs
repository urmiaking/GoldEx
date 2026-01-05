using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class CustomerBalancePageBase : QueryPersistedReportPage<CustomerBalanceFilterVm>
{
    protected List<CustomerRemainingBalanceRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("مانده مشتریان", ClientRoutes.Reporting.CustomerBalances, icon: Icons.Material.Filled.Groups)
    ];

    /* ---------------- Query parameters ---------------- */

    [SupplyParameterFromQuery(Name = "minimumThreshold")]
    public decimal? MinimumThreshold { get; set; }

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "searchQuery")]
    public string? SearchQuery { get; set; }

    [SupplyParameterFromQuery(Name = "type")]
    public string? Type { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        TransactionType? type = null;

        if (!string.IsNullOrWhiteSpace(Type) &&
            Enum.TryParse<TransactionType>(Type, ignoreCase: true, out var parsed))
        {
            type = parsed;
        }

        Filters.MinimumThreshold = MinimumThreshold;
        Filters.SearchQuery = SearchQuery;
        Filters.PriceUnitId = PriceUnitId;
        Filters.Type = type;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.MinimumThreshold,
            Filters.PriceUnitId,
            Filters.SearchQuery,
            Filters.Type,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((CustomerBalanceFilterVm)filter);
    }

    protected async Task LoadReportAsync(CustomerBalanceFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<CustomerRemainingBalanceRpResponse>>(
            action: (s, ct) => s.GetCustomerRemainingBalanceAsync(request, ct));

        IsLoading = false;
    }
}
