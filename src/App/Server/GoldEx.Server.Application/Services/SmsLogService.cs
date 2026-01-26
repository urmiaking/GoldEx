using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.SmsLogs;
using GoldEx.Shared.DTOs.SmsLogs;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class SmsLogService(ISmsLogRepository repository, IMapper mapper) : ISmsLogService
{
    public async Task<PagedList<SmsLogResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new SmsLogsByFilterSpecification(filter)).ToListAsync(cancellationToken);

        var total = await repository.CountAsync(new SmsLogsByFilterSpecification(filter), cancellationToken);

        return new PagedList<SmsLogResponse>
        {
            Data = mapper.Map<List<SmsLogResponse>>(list),
            Total = total,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 100
        };
    }
}