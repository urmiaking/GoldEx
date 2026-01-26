using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.SmsLogs;

namespace GoldEx.Shared.Services.Abstractions;

public interface ISmsLogService
{
    Task<PagedList<SmsLogResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
}