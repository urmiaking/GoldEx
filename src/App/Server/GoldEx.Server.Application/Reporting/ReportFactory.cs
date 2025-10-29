using DevExpress.DataAccess.ObjectBinding;
using DevExpress.Drawing;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.Services;
using DevExpress.XtraReports.UI;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GoldEx.Server.Application.Reporting;

[ScopedService]
internal class ReportFactory(
    IWebHostEnvironment hostingEnvironment,
    IIconService iconService,
    IReportingService reportingService,
    IHttpContextAccessor httpContextAccessor)
    : IReportProviderAsync, IReportProvider
{
    public async Task<XtraReport> GetReportAsync(string id, ReportProviderContext context)
    {
        var parts = id.Split(['?'], 2);
        var reportName = parts[0];
        var queryString = parts.Length > 1 ? parts[1] : string.Empty;

        var reportPath = Path.Combine(hostingEnvironment.ContentRootPath, "Reports", $"{reportName}.repx");
        if (!File.Exists(reportPath))
            throw new NotFoundException($"Report {reportName} not found");

        var report = new XtraReport();
        using (var ms = new MemoryStream(await File.ReadAllBytesAsync(reportPath)))
        {
            report.LoadLayoutFromXml(ms);
        }

        if (reportName == "InvoiceReport")
        {
            // Parse invoiceNumber from query string, e.g., "InvoiceReport?invoiceNumber=123&invoiceType=Purchase"
            var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
            if (long.TryParse(queryParams["invoiceNumber"], out var invoiceNumber) &&
                Enum.TryParse<InvoiceType>(queryParams["invoiceType"], out var invoiceType))
            {
                var response = await reportingService.GetInvoiceReportAsync(invoiceNumber, invoiceType);

                var invoiceUrl = GenerateInvoiceUrl(invoiceNumber, invoiceType); // {schema}://{domain}/invoices/viewer/{invoiceNumber}/{invoiceType}

                var dataSource = new ObjectDataSource
                {
                    Name = "objectDataSource2",
                    DataSource = response, // Set the GetInvoiceReportResponse object
                    DataMember = null
                };

                report.DataSource = dataSource;

                if (report.FindControl("logoBox", true) is XRPictureBox pictureBox)
                {
                    var iconBytes = await iconService.GetIconAsync(IconType.App, Guid.Empty);

                    if (iconBytes is { Length: > 0 })
                    {
                        try
                        {
                            using var ms = new MemoryStream(iconBytes);

                            // WARNING: INTENTIONALLY NOT DISPOSING OF dxImage FOR TESTING
                            var dxImage = DXImage.FromStream(ms);

                            pictureBox.ImageSource = new ImageSource(dxImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to create DXImage from stream: {ex}");
                        }
                    }
                }

                if (report.Parameters["invoiceNumber"] != null)
                {
                    report.Parameters["invoiceNumber"].Value = invoiceNumber;
                    report.Parameters["invoiceUrl"].Value = invoiceUrl;
                }
            }
        }

        return report;
    }

    public XtraReport GetReport(string id, ReportProviderContext context)
    {
        return GetReportAsync(id, context).GetAwaiter().GetResult();
    }

    private string GenerateInvoiceUrl(long invoiceNumber, InvoiceType invoiceType)
    {
        var relativePath = ClientRoutes.Invoices.ViewInvoice.FormatRoute(new { number = invoiceNumber, invoiceType });

        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null || !request.Host.HasValue)
            return relativePath;

        // Respect reverse proxy headers if present
        string GetFirstHeader(string key)
            => request.Headers.TryGetValue(key, out var values) ? values.ToString().Split(',')[0].Trim() : string.Empty;

        var forwardedProto = GetFirstHeader("X-Forwarded-Proto");
        var forwardedHost = GetFirstHeader("X-Forwarded-Host");
        var forwardedPort = GetFirstHeader("X-Forwarded-Port");

        var scheme = !string.IsNullOrWhiteSpace(forwardedProto) ? forwardedProto : request.Scheme;
        var host = !string.IsNullOrWhiteSpace(forwardedHost) ? forwardedHost : request.Host.Value;

        // If forwarded port is present and not already part of host, append it
        if (!string.IsNullOrWhiteSpace(forwardedPort) && !host.Contains(':'))
        {
            host = $"{host}:{forwardedPort}";
        }

        return $"{scheme}://{host}{relativePath}";
    }
}