using GoldEx.Client.Components.Services;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InvoiceFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }

    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public InvoiceType? InvoiceTypeParam { get; set; }
    [Parameter] public TradeScale? TradeScaleParam { get; set; }
    [Parameter] public InvoicePaymentStatus? PaymentStatus { get; set; }
    [Parameter] public string SortLabel { get; set; } = "InvoiceNumber";
    [Parameter] public bool SortDescending { get; set; } = true;

    private string _sortLabel = "InvoiceNumber";
    private bool _sortDescending = true;
    private DateRange? _dateRange;
    private InvoiceType? _invoiceType;
    private TradeScale? _tradeScale;
    private InvoicePaymentStatus? _paymentStatus;

    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _invoiceType = InvoiceTypeParam;
        _tradeScale = TradeScaleParam;
        _paymentStatus = PaymentStatus;
        _sortLabel = SortLabel;
        _sortDescending = SortDescending;
    }

    private string GetInvoiceTypeIcon(InvoiceType? type) => type switch
    {
        InvoiceType.Purchase => Icons.Material.Filled.ArrowUpward,
        InvoiceType.Sell => Icons.Material.Filled.ArrowDownward,
        null => Icons.Material.Filled.CompareArrows,
        _ => Icons.Material.Filled.Help
    };

    private Color GetInvoiceTypeColor(InvoiceType? type) => type switch
    {
        InvoiceType.Purchase => Color.Success,
        InvoiceType.Sell => Color.Error,
        null => Color.Info,
        _ => Color.Default
    };

    private string GetTradeScaleIcon(TradeScale? scale) => scale switch
    {
        TradeScale.Wholesale => Icons.Material.Filled.Apps,
        TradeScale.Retail => Icons.Material.Filled.Square,
        null => Icons.Material.Filled.ViewHeadline,
        _ => Icons.Material.Filled.Help
    };

    private Color GetTradeScaleColor(TradeScale? scale) => scale switch
    {
        TradeScale.Wholesale => Color.Primary,
        TradeScale.Retail => Color.Success,
        null => Color.Info,
        _ => Color.Default
    };

    private string GetPaymentStatusIcon(InvoicePaymentStatus? status) => status switch
    {
        InvoicePaymentStatus.Paid => Icons.Material.Filled.Check,
        InvoicePaymentStatus.HasDebt => Icons.Material.Filled.Pending,
        InvoicePaymentStatus.Overdue => Icons.Material.Filled.MoreTime,
        null => Icons.Material.Filled.ViewHeadline,
        _ => Icons.Material.Filled.Help
    };

    private Color GetPaymentStatusColor(InvoicePaymentStatus? status) => status switch
    {
        InvoicePaymentStatus.Paid => Color.Success,
        InvoicePaymentStatus.HasDebt => Color.Primary,
        InvoicePaymentStatus.Overdue => Color.Error,
        null => Color.Info,
        _ => Color.Default
    };

    private void Apply()
    {
        var result = new InvoicesList.InvoicesMobileFiltersResult(
            new DateRange(_dateRange?.Start, _dateRange?.End),
            _invoiceType,
            _tradeScale,
            _paymentStatus,
            _sortLabel,
            _sortDescending
        );

        Session?.Close(result);
    }

    private void Clear()
    {
        _dateRange = new DateRange();
        _invoiceType = null;
        _tradeScale = null;
        _paymentStatus = null;
        Apply();
    }

    private void Cancel() => Session?.Cancel();
}
