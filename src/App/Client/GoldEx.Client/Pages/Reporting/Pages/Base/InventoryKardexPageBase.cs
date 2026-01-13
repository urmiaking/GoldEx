using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class InventoryKardexPageBase : QueryPersistedReportPage<InventoryKardexFilterVm>
{
    protected List<InventoryKardexRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("کاردکس کالا", ClientRoutes.Reporting.InventoryKardex, icon: Icons.Material.Filled.Inventory2)
    ];

    /* ---------------- Query parameters ---------------- */

    [SupplyParameterFromQuery(Name = "productId")]
    public Guid? ProductId { get; set; }

    [SupplyParameterFromQuery(Name = "coinInstanceId")]
    public Guid? CoinInstanceId { get; set; }

    [SupplyParameterFromQuery(Name = "currencyId")]
    public Guid? CurrencyId { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        Filters.ProductId = ProductId;
        Filters.CoinInstanceId = CoinInstanceId;
        Filters.CurrencyId = CurrencyId;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.ProductId,
            Filters.CoinInstanceId,
            Filters.CurrencyId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((InventoryKardexFilterVm)filter);
    }

    protected async Task LoadReportAsync(InventoryKardexFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<InventoryKardexRpResponse>>(
            action: (s, ct) => s.GetInventoryKardexAsync(request, ct));

        IsLoading = false;
    }
}