using GoldEx.Server.Domain.AppLicenseAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface ILicenseStore
{
    Task<AppLicense?> GetAsync(CancellationToken cancellationToken = default);
    Task SetAsync(AppLicense appLicense, CancellationToken cancellationToken = default);
}