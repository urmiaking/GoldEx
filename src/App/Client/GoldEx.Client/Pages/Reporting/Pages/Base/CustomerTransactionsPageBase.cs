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

public abstract class CustomerTransactionsPageBase : QueryPersistedReportPage<CustomerTransactionFilterVm>
{
    protected List<CustomerTransactionRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("گردش حساب مشتری", ClientRoutes.Reporting.CustomerTransactions, icon: Icons.Material.Filled.ReceiptLong)
    ];

    /* ---------------- Query parameters ---------------- */

    [SupplyParameterFromQuery(Name = "customerId")]
    public Guid? CustomerId { get; set; }

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "role")]
    public string? Role { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        LedgerAccountRole? role = null;

        if (!string.IsNullOrWhiteSpace(Role) &&
            Enum.TryParse<LedgerAccountRole>(Role, ignoreCase: true, out var parsed))
        {
            role = parsed;
        }

        Filters.CustomerId = CustomerId;
        Filters.PriceUnitId = PriceUnitId;
        Filters.Role = role;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.CustomerId,
            Filters.PriceUnitId,
            Filters.Role,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((CustomerTransactionFilterVm)filter);
    }

    protected async Task LoadReportAsync(CustomerTransactionFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<CustomerTransactionRpResponse>>(
            action: (s, ct) => s.GetCustomerTransactionsAsync(request, ct));

        IsLoading = false;
    }
}