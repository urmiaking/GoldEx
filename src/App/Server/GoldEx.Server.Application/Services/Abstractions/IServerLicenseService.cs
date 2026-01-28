using VHDLicenseManager.Requests;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerLicenseService
{
    Task ActivateProductAsync(LicenseCallbackRequest request, CancellationToken cancellationToken = default);
}