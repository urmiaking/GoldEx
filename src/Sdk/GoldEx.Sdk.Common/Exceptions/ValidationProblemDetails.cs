namespace GoldEx.Sdk.Common.Exceptions;

internal class ValidationProblemDetails
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}