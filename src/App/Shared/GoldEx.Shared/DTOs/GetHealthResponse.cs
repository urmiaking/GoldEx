using System.Formats.Asn1;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace GoldEx.Shared.DTOs;

public record HealthCheckResponse
{
    public string? Status { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public Dictionary<string, Entry>? Entries { get; init; }
}

public record Entry
{
    public Dictionary<string, object>? Data { get; init; }
    public string? Description { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Status { get; init; }
    public List<string>? Tags { get; init; }
}