using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Calculator.Client.Pages;

public partial class Simple
{
    private bool _canInstall;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _canInstall = await JsRuntime.InvokeAsync<bool>("getPwaState");
                if (_canInstall)
                {
                    StateHasChanged();
                } 
            }
            catch 
            {
                _canInstall = false;
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task InstallPwa()
    {
        try
        {
            var installed = await JsRuntime.InvokeAsync<bool>("installPwa");
            if (installed)
            {
                _canInstall = false;
                StateHasChanged();
            }
        }
        catch
        {
            AddErrorToast("خطایی در نصب رخ داد");
        }
    }
}