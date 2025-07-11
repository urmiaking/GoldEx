using GoldEx.Shared.DTOs.Reports;

namespace GoldEx.Shared.Services.Abstractions;

public interface IReportService
{
    Task<List<GetReportResponse>> GetListAsync(CancellationToken cancellationToken = default);
}