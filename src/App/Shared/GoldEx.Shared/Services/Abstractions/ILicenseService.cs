using GoldEx.Shared.DTOs.Licenses;

namespace GoldEx.Shared.Services.Abstractions;

public interface ILicenseService
{
    Task<GetLicenseResponse> GetLicenseAsync(CancellationToken cancellationToken = default);
    Task RegisterProductAsync(RegisterProductRequest request, CancellationToken cancellationToken = default);
    Task SendTokenAsync(string phoneNumber, CancellationToken cancellationToken = default);
}