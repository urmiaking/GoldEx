using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class PurchaseInvoicePageBase : QueryPersistedReportPage<PurchaseInvoiceFilterVm>
{
    protected List<PurchaseInvoiceRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("فاکتور خرید", ClientRoutes.Reporting.PurchaseInvoices, icon: Icons.Material.Filled.ShoppingCart)
    ];

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "customerId")]
    public Guid? CustomerId { get; set; }

    [SupplyParameterFromQuery(Name = "paymentStatus")]
    public string? PaymentStatus { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        InvoicePaymentStatus? paymentStatus = null;

        if (!string.IsNullOrWhiteSpace(PaymentStatus) &&
            Enum.TryParse<InvoicePaymentStatus>(PaymentStatus, ignoreCase: true, out var parsed))
        {
            paymentStatus = parsed;
        }

        Filters.PriceUnitId = PriceUnitId;
        Filters.CustomerId = CustomerId;
        Filters.PaymentStatus = paymentStatus;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.PaymentStatus,
            Filters.PriceUnitId,
            Filters.CustomerId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((PurchaseInvoiceFilterVm)filter);
    }

    protected async Task LoadReportAsync(PurchaseInvoiceFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<PurchaseInvoiceRpResponse>>(
            action: (s, ct) => s.GetPurchaseInvoicesAsync(request, ct));

        IsLoading = false;
    }
}