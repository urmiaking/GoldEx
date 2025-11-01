using GoldEx.Shared.DTOs.Settings.Barcodes;

namespace GoldEx.Shared.Services.Abstractions;

public interface IBarcodePrintSettingsService
{
    Task<GetBarcodePrintSettingsResponse> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateBarcodePrintSettingsRequest request, CancellationToken cancellationToken = default);
}