using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Reporting.Pages.Reports;

public partial class InventoryKardex
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task PrintReport()
    {
        var url = ClientRoutes.Reporting.Print.InventoryKardex
            .AppendQueryString(WriteFilterToQuery());

        await JsRuntime.InvokeVoidAsync("openReportPopup", Navigation.ToAbsoluteUri(url).ToString());
    }
}