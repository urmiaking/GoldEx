using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Reporting.Pages.Reports;

public partial class LedgerAccountDetails
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private async Task PrintReport()
    {
        var url = ClientRoutes.Reporting.Print.LedgerAccountDetails
            .AppendQueryString(WriteFilterToQuery());

        await JsRuntime.InvokeVoidAsync("openReportPopup", Navigation.ToAbsoluteUri(url).ToString());
    }

    private void OpenReference((string RefType, Guid? RefId) reference)
    {
        if (!reference.RefId.HasValue)
            return;

        switch (reference.RefType)
        {
            case nameof(LedgerAccountStatementRpResponse.InvoiceId):
                {
                    var url = ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
            case nameof(LedgerAccountStatementRpResponse.PaymentVoucherId):
                {
                    var url = ClientRoutes.Finances.PaymentVouchers.AppendQueryString(new { q = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
            case nameof(LedgerAccountStatementRpResponse.InventoryEntryId):
                {
                    var url = ClientRoutes.InventoryStocks.InventoryEntry.List.AppendQueryString(new { q = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
            case nameof(LedgerAccountStatementRpResponse.InventoryExitId):
                {
                    var url = ClientRoutes.InventoryStocks.InventoryExits.List.AppendQueryString(new { q = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
            case nameof(LedgerAccountStatementRpResponse.InventoryStockId):
                {
                    var url = ClientRoutes.InventoryStocks.List.AppendQueryString(new { q = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
            case nameof(LedgerAccountStatementRpResponse.MeltingBatchId):
                {
                    var url = ClientRoutes.InventoryStocks.MeltingBatches.Index.AppendQueryString(new { q = reference.RefId.Value });
                    Navigation.NavigateTo(url);
                    break;
                }
        }
    }
}