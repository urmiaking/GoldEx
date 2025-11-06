using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IIconService
{
    /// <summary>
    /// Gets the icon for the specified type.
    /// </summary>
    /// <param name="iconType">The type of the icon.</param>
    /// <param name="id">id of the asset</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The icon as a byte array.</returns>
    Task<byte[]?> GetIconAsync(IconType iconType, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the icon path for the specified type.
    /// </summary>
    /// <param name="iconType"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    string? GetIconPath(IconType iconType, Guid id);
}