using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Home.Components;

public partial class RecentInvoicesOverview
{
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;

    private List<GetInvoiceListResponse> _invoices = [];
    private int _totalInvoicesCount;

    private int TotalInvoicesCount => _totalInvoicesCount > 0 ? _totalInvoicesCount : _invoices.Count;
    private int SellCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Sell);
    private int PurchaseCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Purchase);
    private int PaidCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.Paid);
    private int DebtCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.HasDebt);
    private int OverdueCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.Overdue);

    private static DateOnly TodayDate => DateOnly.FromDateTime(DateTime.Today);

    private int TodaySellCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceDate == TodayDate);
    private int TodayPurchaseCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceDate == TodayDate);

    private decimal TodaySellVolume => _invoices.Where(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceDate == TodayDate).Sum(x => x.TotalAmount);
    private decimal TodayPurchaseVolume => _invoices.Where(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceDate == TodayDate).Sum(x => x.TotalAmount);

    private decimal TotalSellVolume => _invoices.Where(x => x.InvoiceType == InvoiceType.Sell).Sum(x => x.TotalAmount);
    private decimal TotalPurchaseVolume => _invoices.Where(x => x.InvoiceType == InvoiceType.Purchase).Sum(x => x.TotalAmount);
    private decimal TotalRemainingDebt => _invoices.Sum(x => x.TotalUnpaidAmount);
    private decimal AverageInvoiceValue => TotalInvoicesCount > 0 ? _invoices.Average(x => x.TotalAmount) : 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInvoicesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInvoicesAsync()
    {
        var filter = new RequestFilter(0, 200, null, null, Sdk.Common.Definitions.SortDirection.Descending);
        var invoiceFilter = new InvoiceFilter(null, null, null, null, null);

        await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceListResponse>>(
            action: (service, token) => service.GetListAsync(filter, invoiceFilter, null, token),
            afterSend: response =>
            {
                _invoices = response.Data;
                _totalInvoicesCount = response.Total;
            },
            createScope: true
        );
    }
}
