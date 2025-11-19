using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerPriceUnitService
{
    Task<GetPriceUnitTitleResponse> GetAsync(string title, CancellationToken cancellationToken = default);
}