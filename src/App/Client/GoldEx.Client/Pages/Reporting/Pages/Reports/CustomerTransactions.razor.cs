using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Reporting.Pages.Reports;

public partial class CustomerTransactions
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task PrintReport()
    {
        var url = ClientRoutes.Reporting.Print.CustomerTransactions
            .AppendQueryString(WriteFilterToQuery());

        await JsRuntime.InvokeVoidAsync("openReportPopup", Navigation.ToAbsoluteUri(url).ToString());
    }

    private void OpenReference((string RefType, Guid? RefId) reference)
    {
        if (!reference.RefId.HasValue)
            return;

        switch (reference.RefType)
        {
            case nameof(CustomerTransactionRpResponse.InvoiceId):
            {
                var url = ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = reference.RefId.Value });
                Navigation.NavigateTo(url);
                break;
            }
            case nameof(CustomerTransactionRpResponse.PaymentVoucherId):
            {
                var url = ClientRoutes.PaymentVouchers.Index.AppendQueryString(new { q = reference.RefId.Value });
                Navigation.NavigateTo(url);
                break;
            }
        }
    }
}