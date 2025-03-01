namespace GoldEx.Sdk.Server.Application.Extensions;

public static class ImageToBase64Converter
{
    public static async Task<(string? Base64String, string? ContentType)> ConvertImageUrlToBase64(string imageUrl)
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

    public static string? GenerateBase64ImageSrc(string base64String, string contentType)
    {
        if (string.IsNullOrEmpty(base64String) || string.IsNullOrEmpty(contentType))
        {
            return null;
        }
        return $"data:{contentType};base64,{base64String}";
    }
}