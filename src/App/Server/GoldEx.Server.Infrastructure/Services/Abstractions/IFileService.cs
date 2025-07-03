namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface IFileService
{
    /// <summary>
    /// Saves a file and returns its web-accessible URL.
    /// </summary>
    /// <param name="filePath">The relative path of the file (e.g. "/assets/icons/12345.png")</param>
    /// <param name="content">The file content as a byte array.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns></returns>
    Task SaveLocalFileAsync(string filePath, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing file with new content and returns the new file path.
    /// </summary>
    /// <param name="filePath">The relative path of the file (e.g. "/assets/icons/12345.png")</param>
    /// <param name="content">The new file content as a byte array.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns></returns>
    Task ReplaceLocalFileAsync(string filePath, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a local file.
    /// </summary>
    /// <param name="filePath">The root relative path of the file (e.g. "wwwroot/assets/icons/12345.png")</param>
    /// <returns></returns>
    void DeleteLocalFile(string filePath);
}