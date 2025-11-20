using GoldEx.Sdk.Common;
using GoldEx.Server.Application.Utilities;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Files.Base)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
public class FilesController(IWebHostEnvironment environment) : Controller
{
    [HttpGet(ApiRoutes.Files.GetInventoryEntryTemplate)]
    public IActionResult GetInventoryEntryTemplate()
    {
        var filePath = environment.GetInventoryEntryTemplateFilePath();

        return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}