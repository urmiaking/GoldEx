using System.Text.Json;

namespace GoldEx.Server.Infrastructure.Helpers;

public static class FileHelpers
{
    private const int MaxAttempts = 5;
    // Jittered backoff to prevent thundering herd if multiple instances retry at once
    private static readonly Random Rng = new();

    public static async Task AtomicWriteAsync<T>(string path, T data, JsonSerializerOptions options, CancellationToken token = default)
    {
        var dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // 1. Write to a temp file first
        var tmpPath = Path.Combine(dir, $"{Path.GetFileName(path)}.tmp_{Guid.CreateVersion7():N}");

        try
        {
            await WriteWithRetriesAsync(tmpPath, data, options, token);

            // 2. Atomic Swap
            // Move(overwrite: true) is atomic on POSIX and modern Windows (ReFS/NTFS)
            await MoveWithRetriesAsync(tmpPath, path, token);
        }
        catch
        {
            // Cleanup temp file on failure
            if (File.Exists(tmpPath))
            {
                try { File.Delete(tmpPath); } catch { /* Ignore cleanup errors */ }
            }
            throw;
        }
    }

    private static async Task WriteWithRetriesAsync<T>(string path, T data, JsonSerializerOptions options, CancellationToken token)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                // FileShare.None = Lock file while writing so nobody reads half-written data
                await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await JsonSerializer.SerializeAsync(fs, data, options, token);
                await fs.FlushAsync(token);
                return;
            }
            catch (IOException) when (attempt < MaxAttempts)
            {
                await Task.Delay(GetRandomBackoff(attempt), token);
            }
        }
    }

    private static async Task MoveWithRetriesAsync(string src, string dest, CancellationToken token)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                // Overwrite = true performs the atomic replacement
                File.Move(src, dest, overwrite: true);
                return;
            }
            catch (IOException) when (attempt < MaxAttempts)
            {
                // Destination might be locked for reading by another instance
                await Task.Delay(GetRandomBackoff(attempt), token);
            }
        }
    }

    /// <summary>
    /// Reads file with retries to handle transient locks on shared volumes
    /// </summary>
    public static async Task<T?> ReadWithRetriesAsync<T>(string path, JsonSerializerOptions options, CancellationToken token)
    {
        if (!File.Exists(path)) return default;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                // FileShare.Read = Allow others to read, but not write while we are reading
                await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

                // Direct stream deserialization (Memory Efficient)
                return await JsonSerializer.DeserializeAsync<T>(fs, options, token);
            }
            catch (IOException) when (attempt < MaxAttempts)
            {
                await Task.Delay(GetRandomBackoff(attempt), token);
            }
            catch (JsonException)
            {
                // File might be corrupted or empty (0 bytes) due to a crash during previous write
                return default;
            }
        }
        return default;
    }

    private static TimeSpan GetRandomBackoff(int attempt)
    {
        // Base delay * 2^attempt + random jitter (0-100ms)
        var baseDelay = 100 * Math.Pow(2, attempt - 1);
        lock (Rng)
        {
            return TimeSpan.FromMilliseconds(baseDelay + Rng.Next(0, 100));
        }
    }
}