using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class ProductInventoryPageBase : QueryPersistedReportPage<ProductInventoryFilterVm>
{
    protected List<ProductInventoryRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("موجودی کالا", ClientRoutes.Reporting.ProductInventory, icon: Icons.Material.Filled.Warehouse)
    ];

    [SupplyParameterFromQuery(Name = "productCategoryId")]
    public Guid? ProductCategoryId { get; set; }

    [SupplyParameterFromQuery(Name = "itemStatus")]
    public string? ItemStatus { get; set; }

    [SupplyParameterFromQuery(Name = "itemType")]
    public string? ItemType { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    protected override void ReadQueryToFilter()
    {
        ItemType? itemType = null;
        ItemStatus? itemStatus = null;

        if (!string.IsNullOrWhiteSpace(ItemType) &&
            Enum.TryParse<ItemType>(ItemType, ignoreCase: true, out var parsedType))
        {
            itemType = parsedType;
        }

        if (!string.IsNullOrWhiteSpace(ItemStatus) &&
            Enum.TryParse<ItemStatus>(ItemStatus, ignoreCase: true, out var parsedStatus))
        {
            itemStatus = parsedStatus;
        }

        Filters.ProductCategoryId = ProductCategoryId;
        Filters.ItemType = itemType;
        Filters.ItemStatus = itemStatus;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.ProductCategoryId,
            Filters.ItemType,
            Filters.ItemStatus,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((ProductInventoryFilterVm)filter);
    }

    protected async Task LoadReportAsync(ProductInventoryFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<ProductInventoryRpResponse>>(
            action: (s, ct) => s.GetProductInventoryAsync(request, ct));

        IsLoading = false;
    }
}