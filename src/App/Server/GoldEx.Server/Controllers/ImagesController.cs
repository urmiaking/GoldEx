using Azure;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Images.Base)]
public class ImagesController(IImageClientService clientService) : ApiControllerBase
{
    [HttpGet(ApiRoutes.Images.GetImage)]
    public async Task<IActionResult> GetImage(string imageUrl, CancellationToken cancellationToken = default)
    {
        var imageBytes = await clientService.GetImageFileAsync(imageUrl, cancellationToken);

        return imageBytes is null ? NotFound() : File(imageBytes, "image/png");
    }
}