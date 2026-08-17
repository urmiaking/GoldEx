using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings.Components.PersonalAccessTokens;

public partial class RevealTokenDialog
{
    [Parameter] public string Token { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string TokenPrefix { get; set; } = string.Empty;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task CopyTokenToClipboard()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Token);
            AddSuccessToast("کلید دسترسی با موفقیت در کلیپ‌بورد کپی شد.");
        }
        catch
        {
            AddErrorToast("امکان کپی خودکار وجود ندارد. لطفاً متن کلید را به صورت دستی انتخاب و کپی فرمایید.");
        }
    }

    private void Close() => MudDialog.Close();
}
