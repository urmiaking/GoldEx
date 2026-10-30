using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.Invoices.Components;
public partial class InvoicePaymentDeleteDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public string AmountText { get; set; } = string.Empty;
}