using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;
public partial class PaymentVoucherDeleteDialog
{
 [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
 [Parameter] public string VoucherNumber { get; set; } = "";
 [Parameter] public string AmountText { get; set; } = "";
}