using GoldEx.Sdk.Common;
using GoldEx.Server.Application.Reporting;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Components.Invoices;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Net.Codecrete.QrCodeGenerator;
using System.Security.Cryptography;
using System.Text;

namespace GoldEx.Server.Controllers;

[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class InvoicePrintController(
    IInvoicePrintService invoices,
    ISettingService settings,
    StoreContext store,
    IWebHostEnvironment environment,
    IDataProtectionProvider dataProtection) : ControllerBase
{
    private const int GuidLength = 16;
    private readonly IDataProtector _shareProtector = dataProtection.CreateProtector("GoldEx.InvoicePrint.Share.v1");

    [HttpGet(ClientRoutes.Invoices.PrintPreview)]
    public async Task<IResult> PreviewAsync(Guid id, CancellationToken cancellationToken)
    {
        if (store.StoreId is not { } storeId)
            return Results.Forbid();

        var invoice = await invoices.GetPrintDocumentAsync(id, cancellationToken);
        if (invoice is null)
            return Results.NotFound();

        var payload = new byte[2 * GuidLength];
        storeId.TryWriteBytes(payload.AsSpan(0, GuidLength));
        id.TryWriteBytes(payload.AsSpan(GuidLength));
        var token = WebEncoders.Base64UrlEncode(_shareProtector.Protect(payload));
        return await RenderPreviewAsync(invoice, token, isShared: false, cancellationToken);
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Vitrine)]
    [HttpGet(ClientRoutes.Invoices.SharedPrint, Name = nameof(SharedPreviewAsync))]
    public async Task<IResult> SharedPreviewAsync(string token, CancellationToken cancellationToken)
    {
        if (ReadShareToken(token) is not { } share)
            return Results.NotFound();

        var storeSlug = await invoices.GetActiveStoreSlugAsync(share.StoreId, cancellationToken);
        if (storeSlug is null)
            return Results.NotFound();

        store.SetStore(share.StoreId, storeSlug);
        var invoice = await invoices.GetPrintDocumentAsync(share.InvoiceId, cancellationToken);
        if (invoice is null)
            return Results.NotFound();

        var customerCopy = invoice with
        {
            Customer = new InvoicePrintParty(invoice.Customer.Name, null, null),
            Payments = invoice.Payments.Select(payment => payment with { Reference = null }).ToArray(),
        };
        return await RenderPreviewAsync(customerCopy, token, isShared: true, cancellationToken);
    }

    private (Guid StoreId, Guid InvoiceId)? ReadShareToken(string token)
    {
        if (token.Length > 256)
            return null;

        try
        {
            var payload = _shareProtector.Unprotect(WebEncoders.Base64UrlDecode(token));
            return payload.Length == 2 * GuidLength
                ? (new Guid(payload.AsSpan(0, GuidLength)), new Guid(payload.AsSpan(GuidLength)))
                : null;
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            return null;
        }
    }

    private async Task<IResult> RenderPreviewAsync(InvoicePrintDocument invoice, string token, bool isShared, CancellationToken cancellationToken)
    {
        var setting = await settings.GetAsync(cancellationToken);
        var icon = new FileInfo(environment.GetAppIconPath(store.StoreSlug));
        var logoUrl = icon.Exists
            ? $"/uploads/icons/app/{Uri.EscapeDataString(icon.Name)}?v={icon.LastWriteTimeUtc.Ticks}"
            : "/apple-touch-icon-180x180.png";

        var shareUrl = Url.RouteUrl(nameof(SharedPreviewAsync), new { token }, Request.Scheme)!;
        var qrSvg = QrCode.EncodeText(shareUrl, QrCode.Ecc.Medium).ToSvgString(4);
        var qrImage = $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(qrSvg))}";

        Response.Headers["Referrer-Policy"] = "no-referrer";
        return new RazorComponentResult<InvoicePrintPreview>(new
        {
            Invoice = invoice,
            Setting = setting,
            LogoUrl = logoUrl,
            ShareUrl = shareUrl,
            QrImage = qrImage,
            IsShared = isShared,
        });
    }
}
