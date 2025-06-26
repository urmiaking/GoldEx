using GoldEx.Shared.DTOs.Reports;

namespace GoldEx.Shared.Services;

public interface IReportService
{
    Task<List<GetReportResponse>> GetListAsync(CancellationToken cancellationToken = default);
}