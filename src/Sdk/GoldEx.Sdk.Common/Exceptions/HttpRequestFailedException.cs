using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoldEx.Sdk.Common.Exceptions;

public class HttpRequestFailedException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpRequestFailedException(HttpStatusCode statusCode) : this(statusCode, $"Request failed with status {statusCode}.")
    {

    }

    public HttpStatusCode StatusCode { get; } = statusCode;

    public static Exception GetException(HttpStatusCode statusCode, HttpResponseMessage? response)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new HttpRequestAuthenticationFailedException(statusCode),
            HttpStatusCode.Forbidden => new HttpRequestAuthorizationFailedException(statusCode),
            HttpStatusCode.BadRequest => new HttpRequestValidationException(statusCode, response),
            _ => new HttpRequestFailedException(statusCode)
        };
    }
}

public class HttpRequestAuthenticationFailedException(HttpStatusCode statusCode)
    : HttpRequestFailedException(statusCode);

public class HttpRequestAuthorizationFailedException(HttpStatusCode statusCode)
    : HttpRequestFailedException(statusCode);

public class HttpRequestValidationException : HttpRequestFailedException
{
    public Dictionary<string, string[]> Errors { get; private set; } = new();

    public HttpRequestValidationException(HttpStatusCode statusCode, HttpResponseMessage? response) : base(statusCode)
    {
        if (response != null)
        {
            TryReadValidationProblems(response);
        }
    }

    private void TryReadValidationProblems(HttpResponseMessage response)
    {
        string? content = null;

        try
        {
            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            content = reader.ReadToEnd();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var problems = JsonSerializer.Deserialize<ValidationProblemDetails>(content, jsonOptions);

            if (problems is not null && problems.Errors.Any())
            {
                Errors = problems.Errors;
                return;
            }
        }
        catch
        {
            // ignored - fall back below
        }

        // Fallback: if server sent plain text (e.g., InvalidOperationException.Message), surface it as a general error
        if (!string.IsNullOrWhiteSpace(content))
        {
            Errors = new Dictionary<string, string[]>
            {
                { string.Empty, [content] }
            };
        }
    }
}