using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
namespace GoldEx.Client.Pages.InventoryEntry.Components;
public partial class InventoryEntryConfirmDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public string Message { get; set; } = string.Empty;
    [Parameter] public string ActionLabel { get; set; } = "تأیید";
    [Parameter] public bool Destructive { get; set; }
}