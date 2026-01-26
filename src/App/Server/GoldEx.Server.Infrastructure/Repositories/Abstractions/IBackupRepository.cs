namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBackupRepository
{
    Task ValidateBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
    Task RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default);
}