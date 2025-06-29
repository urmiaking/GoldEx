using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace GoldEx.Server.Application.Reporting;

public class ReportStorageExtension : ReportStorageWebExtension
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ReportStorageExtension(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
        var reportDirectory = Path.Combine(hostingEnvironment.ContentRootPath, "Reports");
        if (!Directory.Exists(reportDirectory))
        {
            Directory.CreateDirectory(reportDirectory);
        }
    }

    public override async Task<byte[]> GetDataAsync(string url)
    {
        var reportPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Reports", $"{url}.repx");
        if (File.Exists(reportPath))
            return await File.ReadAllBytesAsync(reportPath);

        throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Could not find report '{url}'.");
    }

    public override byte[] GetData(string url)
    {
        var reportPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Reports", $"{url}.repx");
        if (File.Exists(reportPath))
            return File.ReadAllBytes(reportPath);

        throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Report '{url}' not found.");
    }

    public override void SetData(XtraReport report, string url)
    {
        var reportPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Reports", $"{url}.repx");
        using var ms = new MemoryStream();
        report.SaveLayoutToXml(ms);
        File.WriteAllBytes(reportPath, ms.ToArray());
    }

    public override string SetNewData(XtraReport report, string defaultUrl)
    {
        SetData(report, defaultUrl);
        return defaultUrl;
    }

    public override Dictionary<string, string> GetUrls()
    {
        var reportDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "Reports");
        return Directory.GetFiles(reportDirectory, "*.repx")
            .Select(Path.GetFileNameWithoutExtension).ToDictionary(x => x, x => x == "InvoiceReport" ? "فاکتور فروش" : x);
    }

    public override bool CanSetData(string url) => true;
    public override bool IsValidUrl(string url) => true;
}