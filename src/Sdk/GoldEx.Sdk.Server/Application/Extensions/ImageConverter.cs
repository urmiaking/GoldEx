namespace GoldEx.Sdk.Server.Application.Extensions;

public static class ImageConverter
{
    /// <summary>
    /// Converts an image URL to a Base64 string and its content type.
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
    public static async Task<(string? Base64String, string? ContentType)> ToBase64Async(string imageUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode(); // Ensure we have a successful response

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var base64String = Convert.ToBase64String(imageBytes);
            var contentType = response.Content.Headers.ContentType?.ToString();

            return (base64String, contentType);
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors (e.g., 404, network issues)
            Console.WriteLine($"Error fetching image: {ex.Message}");
            return (null, null); // Or throw, log, etc.
        }
        catch (Exception ex)
        {
            // Handle other exceptions (e.g., invalid URL, memory issues)
            Console.WriteLine($"An error occurred: {ex.Message}");
            return (null, null); // Or throw, log, etc.
        }
    }

    /// <summary>
    /// Downloads an image from a given URL and returns its byte array and format.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <returns>A tuple containing the image byte array and the image format (e.g., "png", "jpeg"). Returns (null, null) if an error occurs.</returns>
    public static async Task<(byte[]? ByteArray, string? ImageFormat)> ToByteArrayAsync(string imageUrl)
    {
        try
        {
            // Using statement ensures HttpClient is disposed of properly.
            using var httpClient = new HttpClient();

            // Send a GET request to the specified URL.
            var response = await httpClient.GetAsync(imageUrl);

            // Throw an HttpRequestException if the response indicates an error.
            response.EnsureSuccessStatusCode();

            // Read the content of the response as a byte array.
            var imageBytes = await response.Content.ReadAsByteArrayAsync();

            // Get the content type header from the response.
            var contentType = response.Content.Headers.ContentType?.ToString();

            string? imageFormat = null;
            if (!string.IsNullOrEmpty(contentType) && contentType.Contains("/"))
            {
                // Split the content type string (e.g., "image/png") by '/'
                // and take the second part (e.g., "png").
                imageFormat = contentType.Split('/')[1];
            }

            return (imageBytes, imageFormat);
        }
        catch (HttpRequestException ex)
        {
            // Handle specific HTTP request errors (e.g., 404 Not Found, network issues).
            Console.WriteLine($"Error fetching image: {ex.Message}");
            // Depending on requirements, you might want to log this error to a file or a logging service.
            return (null, null);
        }
        catch (ArgumentNullException ex)
        {
            // Handle cases where imageUrl might be null or empty if not validated before calling.
            Console.WriteLine($"Invalid URL provided: {ex.Message}");
            return (null, null);
        }
        catch (IndexOutOfRangeException ex)
        {
            // Handle cases where the ContentType might not be in the expected "type/subtype" format.
            Console.WriteLine($"Could not parse image format from ContentType: {ex.Message}");
            // You might want to return the full contentType or a default value here if appropriate.
            return (null, null);
        }
        catch (Exception ex)
        {
            // Handle any other unexpected exceptions (e.g., invalid URL format, memory issues during download).
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return (null, null);
        }
    }

    public static string? GenerateBase64ImageSrc(string base64String, string contentType)
    {
        if (string.IsNullOrEmpty(base64String) || string.IsNullOrEmpty(contentType))
        {
            return null;
        }
        return $"data:{contentType};base64,{base64String}";
    }
}