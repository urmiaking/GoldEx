using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface ILicensePaymentService
{
    Task<List<LicensePaymentResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(LicensePaymentRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, LicensePaymentStatus status, CancellationToken cancellationToken = default);
}