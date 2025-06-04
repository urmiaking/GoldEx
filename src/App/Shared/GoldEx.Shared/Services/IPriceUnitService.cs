using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Shared.Services;

public interface IPriceUnitService
{
    Task<GetPriceUnitResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GetPriceUnitResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<GetPriceUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(CreatePriceUnitRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdatePriceUnitRequest request, CancellationToken cancellationToken = default);
    Task SetStatus(Guid id, UpdatePriceUnitStatusRequest request, CancellationToken cancellationToken = default);
}