using GoldEx.Client.Components.Services;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;
public partial class PaymentVoucherFiltersDialog
{
 [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
 [Parameter] public DateRange DateRange { get; set; } = new();
 [Parameter] public PaymentVoucherType? VoucherType { get; set; }
 [Parameter] public string SortLabel { get; set; } = "PaymentDate";
 [Parameter] public bool SortDescending { get; set; } = true;
 private DateRange _range = new(); private PaymentVoucherType? _type;
 private string _sort = "PaymentDate"; private bool _desc = true;
 public record FilterResult(DateRange DateRange, PaymentVoucherType? VoucherType, string SortLabel, bool SortDescending);
 protected override void OnParametersSet() { _range = new(DateRange.Start,DateRange.End); _type = VoucherType; _sort = SortLabel; _desc = SortDescending; }
 private void Apply() => Session?.Close(new FilterResult(new(_range.Start,_range.End),_type,_sort,_desc));
 private void Clear() { _range = new(); _type = null; Apply(); }
}