using System.Net.Sockets;

namespace GoldEx.Sdk.Client.Extensions;

public static class HttpRequestExtensions
{
    // Helper method to detect connection refused
    public static bool IsConnectionRefused(this HttpRequestException ex)
    {
        // In Blazor WASM, "Failed to fetch" indicates a network error like connection refused
        if (ex.Message.Contains("Failed to fetch"))
        {
            Console.WriteLine("Detected 'Failed to fetch' - treating as connection refused.");
            return true;
        }

        // Fallback for environments with SocketException (e.g., non-WASM)
        if (ex.InnerException is SocketException socketEx)
        {
            Console.WriteLine($"SocketErrorCode: {socketEx.SocketErrorCode}");
            return socketEx.SocketErrorCode == SocketError.ConnectionRefused;
        }

        return false;
    }
}