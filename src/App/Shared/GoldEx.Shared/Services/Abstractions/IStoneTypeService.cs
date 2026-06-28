using GoldEx.Shared.DTOs.StoneTypes;

namespace GoldEx.Shared.Services.Abstractions;

public interface IStoneTypeService
{
    Task<List<GetStoneTypeResponse>> GetListAsync(StoneTypeRequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetStoneTypeResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateStoneTypeRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateStoneTypeRequest request, CancellationToken cancellationToken = default);
    Task ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
