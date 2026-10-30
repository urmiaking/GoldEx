using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Components;
public partial class CustomerTransferDeleteDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public string VoucherNumber { get; set; } = "";
    [Parameter] public string AmountDescription { get; set; } = "";
}