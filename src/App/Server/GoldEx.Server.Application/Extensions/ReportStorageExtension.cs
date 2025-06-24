using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Server.Domain.ReportLayoutAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ReportLayouts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application.Extensions;

[ScopedService]
public class ReportStorageExtension(IServiceProvider serviceProvider, IWebHostEnvironment hostingEnvironment)
    : ReportStorageWebExtension
{
    public override async Task<byte[]> GetDataAsync(string url)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        // 1. Try to get the report from the database.
        var reportLayout = await reportRepository.Get(new ReportLayoutsByNameSpecification(url)).FirstOrDefaultAsync();
        if (reportLayout != null)
            return reportLayout.LayoutData;

        // 2. If not in DB, fall back to the initial .repx template on disk.
        // This is crucial for loading the default template the first time.
        var reportPath = Path.Combine(hostingEnvironment.ContentRootPath, "Reports", $"{url}.repx");
        if (File.Exists(reportPath))
            return await File.ReadAllBytesAsync(reportPath);

        throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Could not find report '{url}'.");
    }

    public override async Task<Dictionary<string, string>> GetUrlsAsync()
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        return await reportRepository.Get(new ReportLayoutsDefaultSpecification())
            .ToDictionaryAsync(r => r.Name, r => r.DisplayName);
    }

    public override async Task SetDataAsync(XtraReport report, Stream stream)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        // Find the report in the database.
        var reportLayout = await reportRepository.Get(new ReportLayoutsByNameSpecification(report.Name)).FirstOrDefaultAsync();

        if (reportLayout != null)
        {
            // If it exists, update its layout data and modified date.
            reportLayout.SetLayoutData(stream.ToByteArray());
            await reportRepository.UpdateAsync(reportLayout);
        }
        else
        {
            // If it's a new report, create a new entry.
            // We use the 'url' as both the Name and DisplayName for simplicity here.
            // You might want a more sophisticated UI to set a custom DisplayName.
            reportLayout = ReportLayout.Create(report.Name, report.DisplayName, stream.ToByteArray());
            
            await reportRepository.CreateAsync(reportLayout);
        }
    }

    public override byte[] GetData(string url)
    {
        using var scope = serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        var reportLayout = repo.Get(new ReportLayoutsByNameSpecification(url)).FirstOrDefault();
        if (reportLayout != null) 
            return reportLayout.LayoutData;

        var reportPath = Path.Combine(hostingEnvironment.ContentRootPath, "Reports", $"{url}.repx");
        if (File.Exists(reportPath))
            return File.ReadAllBytes(reportPath);

        throw new DevExpress.XtraReports.Web.ClientControls.FaultException($"Report '{url}' not found.");
    }

    public override async void SetData(XtraReport report, string url)
    {
        using var scope = serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        using var ms = new MemoryStream();
        report.SaveLayoutToXml(ms);
        var content = ms.ToArray();

        var existing = repo.Get(new ReportLayoutsByNameSpecification(url)).FirstOrDefault();
        if (existing != null)
        {
            existing.SetLayoutData(content);
            await repo.UpdateAsync(existing);
        }
        else
        {
            var newLayout = ReportLayout.Create(url, report.DisplayName, content);
            
            await repo.CreateAsync(newLayout);
        }
    }

    public override string SetNewData(XtraReport report, string defaultUrl)
    {
        SetData(report, defaultUrl);
        return defaultUrl;
    }

    public override Dictionary<string, string> GetUrls()
    {
        using var scope = serviceProvider.CreateScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        return reportRepository.Get(new ReportLayoutsDefaultSpecification())
            .ToDictionary(r => r.Name, r => r.DisplayName);
    }

    public override bool CanSetData(string url) => true;
    public override bool IsValidUrl(string url) => true;
}