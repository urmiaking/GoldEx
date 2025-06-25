using DevExpress.XtraReports.Services;
using DevExpress.XtraReports.UI;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ReportLayouts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application.Reporting;

[ScopedService]
internal class ReportFactory(IServiceProvider serviceProvider, IWebHostEnvironment hostingEnvironment) : IReportProviderAsync, IReportProvider
{
    public async Task<XtraReport> GetReportAsync(string id, ReportProviderContext context)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        var layout = await repo.Get(new ReportLayoutsByNameSpecification(id)).FirstOrDefaultAsync();
        if (layout == null)
            throw new Exception($"Report '{id}' not found.");

        using var ms = new MemoryStream(layout.LayoutData);
        var report = new XtraReport();
        report.LoadLayoutFromXml(ms);

        report.DisplayName = layout.DisplayName;
        report.Name = layout.Name;

        // If you want to bind data manually or inject logic, do it here:
        // report.DataSource = await GetDataAsync(layout.Name);

        return report;
    }

    public XtraReport GetReport(string id, ReportProviderContext context)
    {
        var parts = id.Split(['?'], 2);
        var reportName = parts[0];
        var queryString = parts.Length > 1 ? parts[1] : string.Empty;

        using var scope = serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportLayoutRepository>();

        var layout = repo.Get(new ReportLayoutsByNameSpecification(reportName)).FirstOrDefault();
        if (layout != null)
        {
            using var ms = new MemoryStream(layout.LayoutData);
            var report = new XtraReport();
            report.LoadLayoutFromXml(ms);

            report.DisplayName = layout.DisplayName;
            report.Name = layout.Name;

            // If you want to bind data manually or inject logic, do it here:
            // report.DataSource = await GetDataAsync(layout.Name);

            return report;
        }

        var reportPath = Path.Combine(hostingEnvironment.ContentRootPath, "Reports", $"{reportName}.repx");
        if (File.Exists(reportPath))
        {
            var bytes = File.ReadAllBytes(reportPath);

            using var ms = new MemoryStream(bytes);
            var report = new XtraReport();
            report.LoadLayoutFromXml(ms);

            report.DisplayName = id;
            report.Name = id;

            return report;
        }

        throw new Exception($"Report '{id}' not found.");
    }
}