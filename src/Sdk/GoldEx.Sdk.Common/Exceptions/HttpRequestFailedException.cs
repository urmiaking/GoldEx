using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoldEx.Sdk.Common.Exceptions;

public class HttpRequestFailedToFetchException : Exception
{
    public HttpRequestFailedToFetchException(string message) : base(message)
    {
        
    }

    public HttpRequestFailedToFetchException()
    {
        
    }
}

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
        try
        {
            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var problems = JsonSerializer.Deserialize<ValidationProblemDetails>(content, jsonOptions);

            if (problems != null)
                Errors = problems.Errors;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}