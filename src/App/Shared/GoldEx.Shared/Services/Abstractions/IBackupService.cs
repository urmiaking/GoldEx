using GoldEx.Shared.DTOs.Backups;

namespace GoldEx.Shared.Services.Abstractions;

public interface IBackupService
{
    Task<string?> GetBackupFilePathAsync(CancellationToken cancellationToken = default);
    Task RestoreDatabaseAsync(RestoreDatabaseRequest request, CancellationToken cancellationToken = default);
}