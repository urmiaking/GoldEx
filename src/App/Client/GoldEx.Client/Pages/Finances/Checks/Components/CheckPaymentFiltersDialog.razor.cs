using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class CheckPaymentFiltersDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public CheckPaymentStatus? PaymentStatus { get; set; }

    private DateRange _dateRange = new();
    private CheckPaymentStatus? _paymentStatus;

    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _paymentStatus = PaymentStatus;
    }

    private string GetPaymentStatusIcon(CheckPaymentStatus? status) => status switch
    {
        CheckPaymentStatus.Accepted => Icons.Material.Filled.Check,
        CheckPaymentStatus.Pending => Icons.Material.Filled.Pending,
        CheckPaymentStatus.Returned => Icons.Material.Filled.Error,
        null => Icons.Material.Filled.ViewHeadline,
        _ => Icons.Material.Filled.Help
    };

    private Color GetPaymentStatusColor(CheckPaymentStatus? status) => status switch
    {
        CheckPaymentStatus.Accepted => Color.Success,
        CheckPaymentStatus.Pending => Color.Warning,
        CheckPaymentStatus.Returned => Color.Error,
        null => Color.Info,
        _ => Color.Default
    };

    private void Apply()
    {
        var result = new CheckPaymentsList.CheckPaymentsMobileFiltersResult(
            new DateRange(_dateRange.Start, _dateRange.End),
            _paymentStatus
        );

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Clear()
    {
        _dateRange = new DateRange();
        _paymentStatus = null;
        Apply();
    }

    private void Cancel() => MudDialog.Cancel();
}
