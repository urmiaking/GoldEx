namespace GoldEx.Shared.Services;

public interface IImageClientService
{
    Task<byte[]?> GetImageFileAsync(string imageUrl, CancellationToken cancellationToken = default);
}