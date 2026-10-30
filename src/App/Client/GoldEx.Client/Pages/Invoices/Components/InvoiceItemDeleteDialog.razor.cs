using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.Invoices.Components;
public partial class InvoiceItemDeleteDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public string ItemTitle { get; set; } = string.Empty;
}