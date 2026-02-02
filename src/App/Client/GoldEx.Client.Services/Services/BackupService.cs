using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Backups;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class BackupService(HttpClient client) : IBackupService
{
    public Task<string?> GetBackupFilePathAsync(CancellationToken cancellationToken = default) 
        => Task.FromResult(ApiUrls.Backups.GetFilePath())!;

    public async Task RestoreDatabaseAsync(RestoreDatabaseRequest request, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        content.Add(
            new StreamContent(request.BackupStream),
            "file",
            request.FileName);

        using var response = await client.PostAsync(
            ApiUrls.Backups.Restore(),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}