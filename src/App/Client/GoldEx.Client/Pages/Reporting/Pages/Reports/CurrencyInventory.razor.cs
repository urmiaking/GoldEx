using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Reporting.Pages.Reports;

public partial class CurrencyInventory
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task PrintReport()
    {
        var url = ClientRoutes.Reporting.Print.CurrencyInventory
            .AppendQueryString(WriteFilterToQuery());

        await JsRuntime.InvokeVoidAsync("openReportPopup", Navigation.ToAbsoluteUri(url).ToString());
    }
}