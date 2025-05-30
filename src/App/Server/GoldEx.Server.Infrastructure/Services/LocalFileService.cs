using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Infrastructure.Services;

[ScopedService]
internal class LocalFileService(ILogger<LocalFileService> logger) : IFileService
{
    private readonly ILogger<LocalFileService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task SaveLocalFileAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        if (content == null || content.Length == 0)
            throw new ArgumentException("File content cannot be empty.", nameof(content));

        var rootFilePath = Path.Combine("wwwroot", filePath);
        var directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directory))
            throw new InvalidOperationException("Invalid file directory path.");

        try
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(rootFilePath, content, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _logger.LogError(ex, "Failed to save file: {rootFilePath}", rootFilePath);
            throw new ApplicationException("Failed to save file.", ex);
        }
    }

    public async Task ReplaceLocalFileAsync(string filePath, byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("New file path cannot be empty.");

        if (content == null || content.Length == 0)
            throw new ArgumentException("File content cannot be empty.", nameof(content));

        var fileDirectory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(fileDirectory))
            throw new InvalidOperationException("Invalid new file directory path.");

        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            await File.WriteAllBytesAsync(filePath, content, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _logger.LogError(ex, "Failed to replace file: {filePath}", filePath);
            throw new ApplicationException("Failed to replace file.", ex);
        }
    }

    public void DeleteLocalFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
        }
    }
}