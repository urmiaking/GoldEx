using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Reporting;
using GoldEx.Shared.DTOs.Reports;
using GoldEx.Shared.Services;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportService(ReportStorageExtension reportStorage) : IReportService
{
    public Task<List<GetReportResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var reportUrls = reportStorage.GetUrls();

        var response = reportUrls.Select(kvp => 
            new GetReportResponse(kvp.Key, kvp.Value));

        return Task.FromResult(response.ToList());
    }
}