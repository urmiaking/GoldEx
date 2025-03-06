using GoldEx.Client.Shared.DTOs;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface ICheckpointLocalClientService
{
    Task<GetCheckPointResponse?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default);
    Task AddCheckPointAsync(string entityName, CancellationToken cancellationToken = default);
}