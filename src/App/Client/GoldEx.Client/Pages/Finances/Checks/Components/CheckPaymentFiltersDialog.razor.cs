using GoldEx.Client.Components.Services;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace GoldEx.Client.Pages.Finances.Checks.Components;
public partial class CheckPaymentFiltersDialog
{
 [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
 [Parameter] public DateRange DateRange { get; set; } = new();
 [Parameter] public CheckPaymentStatus? PaymentStatus { get; set; }
 [Parameter] public string SortLabel { get; set; } = "DueDate";
 [Parameter] public bool SortDescending { get; set; } = true;
 private DateRange _dateRange = new();
 private CheckPaymentStatus? _paymentStatus;
 private string _sortLabel = "DueDate";
 private bool _descending = true;
 public record FilterResult(DateRange DateRange, CheckPaymentStatus? PaymentStatus, string SortLabel, bool SortDescending);
 protected override void OnParametersSet()
 {
  _dateRange = new(DateRange.Start, DateRange.End);
  _paymentStatus = PaymentStatus;
  _sortLabel = SortLabel;
  _descending = SortDescending;
 }
 private string GetPaymentStatusIcon(CheckPaymentStatus? status) => status switch
 {
  CheckPaymentStatus.Accepted => Icons.Material.Filled.Check,
  CheckPaymentStatus.Pending => Icons.Material.Filled.Pending,
  CheckPaymentStatus.Returned => Icons.Material.Filled.Error,
  _ => Icons.Material.Filled.ViewHeadline
 };
 private void Apply() => Session?.Close(new FilterResult(new(_dateRange.Start,_dateRange.End),_paymentStatus,_sortLabel,_descending));
 private void Clear() { _dateRange = new(); _paymentStatus = null; Apply(); }
 private void Cancel() => Session?.Cancel();
}