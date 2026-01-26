using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Backups;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Backups.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class BackupsController(IBackupService service) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Backups.GetFile)]
    public async Task<IActionResult> DownloadAsync()
    {
        var filePath = await service.GetBackupFilePathAsync();

        if (filePath is null)
            return NotFound();

        return PhysicalFile(
            filePath,
            "application/octet-stream",
            Path.GetFileName(filePath));
    }

    [HttpPost(ApiRoutes.Backups.Restore)]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> RestoreAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            return BadRequest();

        await using var stream = file.OpenReadStream();

        var request = new RestoreDatabaseRequest(
            stream,
            file.FileName);

        await service.RestoreDatabaseAsync(request, cancellationToken);

        return Accepted();
    }
}