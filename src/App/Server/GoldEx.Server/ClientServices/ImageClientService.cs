using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Services;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class ImageClientService(HttpClient httpClient) : IImageClientService
{
    public async Task<byte[]?> GetImageFileAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            imageUrl = Uri.UnescapeDataString(imageUrl);

            var response = await httpClient.GetAsync(imageUrl, cancellationToken);

            if (!response.IsSuccessStatusCode) 
                return null;

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return imageBytes;

        }
        catch
        {
            return null;
        }
    }
}