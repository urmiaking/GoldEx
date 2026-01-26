using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class PaymentPageBase : QueryPersistedReportPage<PaymentFilterVm>
{
    protected List<PaymentRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("پرداخت/دریافت", ClientRoutes.Reporting.Payments, icon: Icons.Material.Filled.Payment)
    ];

    [SupplyParameterFromQuery(Name = "priceUnitId")]
    public Guid? PriceUnitId { get; set; }

    [SupplyParameterFromQuery(Name = "customerId")]
    public Guid? CustomerId { get; set; }

    [SupplyParameterFromQuery(Name = "paymentSide")]
    public string? PaymentSide { get; set; }

    [SupplyParameterFromQuery(Name = "paymentType")]
    public string? PaymentType { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        PaymentSide? paymentSide = null;
        PaymentType? paymentType = null;

        if (!string.IsNullOrWhiteSpace(PaymentSide) &&
            Enum.TryParse<PaymentSide>(PaymentSide, ignoreCase: true, out var parsedSide))
        {
            paymentSide = parsedSide;
        }

        if (!string.IsNullOrWhiteSpace(PaymentType) &&
            Enum.TryParse<PaymentType>(PaymentType, ignoreCase: true, out var parsedType))
        {
            paymentType = parsedType;
        }

        Filters.PriceUnitId = PriceUnitId;
        Filters.CustomerId = CustomerId;
        Filters.PaymentSide = paymentSide;
        Filters.PaymentType = paymentType;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.PaymentType,
            Filters.PaymentSide,
            Filters.PriceUnitId,
            Filters.CustomerId,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((PaymentFilterVm)filter);
    }

    protected async Task LoadReportAsync(PaymentFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<PaymentRpResponse>>(
            action: (s, ct) => s.GetPaymentsAsync(request, ct));

        IsLoading = false;
    }
}