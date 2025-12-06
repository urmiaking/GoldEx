using GoldEx.Shared.DTOs.InventoryExits;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryExitService
{
    Task ExitAsync(CreateInventoryExitRequest request, CancellationToken cancellationToken = default);
}