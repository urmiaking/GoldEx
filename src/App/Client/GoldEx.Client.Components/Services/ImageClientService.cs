using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Components.Services;

public class ImageClientService(HttpClient client) : IImageClientService
{
    public async Task<byte[]?> GetImageFileAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Images.GetImage(imageUrl), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}