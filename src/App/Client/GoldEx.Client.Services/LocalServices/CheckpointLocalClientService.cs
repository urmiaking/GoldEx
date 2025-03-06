using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Application.Abstractions;
using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Client.Shared.DTOs;
using GoldEx.Sdk.Common.DependencyInjections;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class CheckpointLocalClientService(IMapper mapper, ICheckpointService service) : ICheckpointLocalClientService
{
    public async Task<GetCheckPointResponse?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default)
    {
        var item = await service.GetLastCheckPointAsync(entityName, cancellationToken);

        return item is null ? null : mapper.Map<GetCheckPointResponse>(item);
    }

    public async Task AddCheckPointAsync(string entityName, CancellationToken cancellationToken = default)
    {
        var checkPoint = new Checkpoint(entityName);

        await service.AddCheckPointAsync(checkPoint, cancellationToken);
    }
}