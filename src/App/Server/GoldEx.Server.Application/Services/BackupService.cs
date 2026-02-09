using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.Backups;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Backups;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class BackupService(
    IOptions<BackupSettings> options,
    ILogger<BackupService> logger,
    IHostEnvironment environment,
    IBackupRepository repository,
    RestoreDatabaseValidator validator) : IBackupService
{
    private readonly BackupSettings _backupSettings = options.Value;
    private readonly string _backupDirectory = Path.Combine(environment.ContentRootPath, "backups");

    public Task<string?> GetBackupFilePathAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Directory
            .EnumerateFiles(_backupDirectory, $"{_backupSettings.DbPrefix}*.bak")
            .OrderByDescending(f => f, StringComparer.Ordinal)
            .FirstOrDefault());
    }

    public async Task RestoreDatabaseAsync(RestoreDatabaseRequest request, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_backupDirectory);

        var tempFilePath = Path.Combine(
            _backupDirectory,
            $"{Guid.CreateVersion7()}.bak");

        await using (var fileStream = File.Create(tempFilePath))
        {
            await request.BackupStream.CopyToAsync(fileStream, cancellationToken);
        }

        try
        {
            await validator.ValidateAndThrowAsync(tempFilePath, cancellationToken);
            await repository.RestoreAsync(tempFilePath, cancellationToken);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw new ValidationException(new List<ValidationFailure> { new("backup", e.Message) });
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }
}