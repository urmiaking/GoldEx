using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.InventoryStocks.Components;
public partial class MeltingBatchConfirmDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public string ActionLabel { get; set; } = "تأیید";
}