using GoldEx.Shared.DTOs.AppReleases;

namespace GoldEx.Shared.Services.Abstractions;

public interface IAppReleaseService
{
    Task<List<AppReleaseResponse>> GetListAsync(CancellationToken cancellationToken = default);
}