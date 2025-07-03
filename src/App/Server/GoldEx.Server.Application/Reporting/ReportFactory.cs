using DevExpress.DataAccess.ObjectBinding;
using DevExpress.Drawing;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.Services;
using DevExpress.XtraReports.UI;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Reporting;

[ScopedService]
internal class ReportFactory(
    IWebHostEnvironment hostingEnvironment,
    IIconService iconService,
    IReportingService reportingService)
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
            // Parse invoiceNumber from query string, e.g., "InvoiceReport?invoiceNumber=123"
            var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
            if (long.TryParse(queryParams["invoiceNumber"], out var invoiceNumber))
            {
                // Fetch data from IReportingService
                var response = await reportingService.GetInvoiceReportAsync(invoiceNumber);
                var iconPath = await iconService.GetIconAsync(IconType.App, Guid.Empty);

                // Create ObjectDataSource
                var dataSource = new ObjectDataSource
                {
                    Name = "objectDataSource2",
                    DataSource = response, // Set the GetInvoiceReportResponse object
                    DataMember = null // Set to null for a single object
                };

                // Assign data source to the report
                report.DataSource = dataSource;

                if (report.FindControl("logoBox", true) is XRPictureBox pictureBox)
                {
                    var iconBytes = await iconService.GetIconAsync(IconType.App, Guid.Empty);

                    if (iconBytes != null && iconBytes.Length > 0)
                    {
                        try
                        {
                            using var ms = new MemoryStream(iconBytes);

                            // WARNING: INTENTIONALLY NOT DISPOSING OF dxImage FOR TESTING
                            var dxImage = DevExpress.Drawing.DXImage.FromStream(ms);

                            pictureBox.ImageSource = new DevExpress.XtraPrinting.Drawing.ImageSource(dxImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to create DXImage from stream: {ex.ToString()}");
                        }
                    }
                }


                // Map report parameter to invoiceNumber (if defined in the report)
                if (report.Parameters["invoiceNumber"] != null)
                {
                    report.Parameters["invoiceNumber"].Value = invoiceNumber;
                }
            }
        }

        return report;
    }

    public XtraReport GetReport(string id, ReportProviderContext context)
    {
        // Synchronous version (optional, implement if needed)
        return GetReportAsync(id, context).GetAwaiter().GetResult();
    }
}