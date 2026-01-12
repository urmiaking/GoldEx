using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Base;

public abstract class InvoicePaymentPageBase : QueryPersistedReportPage<InvoicePaymentFilterVm>
{
    protected List<InvoicePaymentRpResponse>? Data;

    protected readonly List<BreadcrumbItem> Breadcrumbs =
    [
        new("صفحه اصلی", ClientRoutes.Home.Index, icon:  Icons.Material.Filled.Home),
        new("گزارش ها", ClientRoutes.Reporting.Index, icon: Icons.Material.Filled.BarChart),
        new("ریز پرداخت ها", ClientRoutes.Reporting.InvoicePayments, icon: Icons.Material.Filled.Payments)
    ];
    
    [SupplyParameterFromQuery(Name = "invoiceType")]
    public string? InvoiceType { get; set; }

    [SupplyParameterFromQuery(Name = "invoiceNumber")]
    public long? InvoiceNumber { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public DateTime? Start { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public DateTime? End { get; set; }

    /* ---------------- Mapping ---------------- */

    protected override void ReadQueryToFilter()
    {
        InvoiceType? invoiceType = null;

        if (!string.IsNullOrWhiteSpace(InvoiceType) &&
            Enum.TryParse<InvoiceType>(InvoiceType, ignoreCase: true, out var parsedSide))
        {
            invoiceType = parsedSide;
        }

        Filters.InvoiceNumber = InvoiceNumber;
        Filters.InvoiceType = invoiceType;
        Filters.DateRange = DateRangeExtensions.From(Start, End);
    }

    protected override object WriteFilterToQuery()
        => new
        {
            Filters.InvoiceNumber,
            Filters.InvoiceType,
            Filters.DateRange?.Start,
            Filters.DateRange?.End
        };

    /* ---------------- Actions ---------------- */

    protected async Task OnSubmit(ReportFilterVmBase filter)
    {
        PersistFiltersToQuery();
        await LoadReportAsync((InvoicePaymentFilterVm)filter);
    }

    protected async Task LoadReportAsync(InvoicePaymentFilterVm model)
    {
        var request = model.ToRequest();

        IsLoading = true;

        Data = await SendRequestAsync<IReportingService, List<InvoicePaymentRpResponse>>(
            action: (s, ct) => s.GetInvoicePaymentsAsync(request, ct));

        IsLoading = false;
    }
}