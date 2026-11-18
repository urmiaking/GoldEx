using GoldEx.Sdk.Common;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Components.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class InvoicePrintController(
    IInvoicePrintService invoices,
    ISettingService settings,
    IStoreContext store,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet(ClientRoutes.Invoices.PrintPreview)]
    public async Task<IResult> PreviewAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!store.StoreId.HasValue)
            return Results.Forbid();

        var invoice = await invoices.GetPrintDocumentAsync(id, cancellationToken);
        if (invoice is null)
            return Results.NotFound();

        var setting = await settings.GetAsync(cancellationToken);
        var icon = new FileInfo(environment.GetAppIconPath(store.StoreSlug));
        var logoUrl = icon.Exists
            ? $"/uploads/icons/app/{Uri.EscapeDataString(icon.Name)}?v={icon.LastWriteTimeUtc.Ticks}"
            : "/apple-touch-icon-180x180.png";

        return new RazorComponentResult<InvoicePrintPreview>(new
        {
            Invoice = invoice,
            Setting = setting,
            LogoUrl = logoUrl,
        });
    }
}
