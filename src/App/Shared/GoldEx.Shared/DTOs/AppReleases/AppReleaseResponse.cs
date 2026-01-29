namespace GoldEx.Shared.DTOs.AppReleases;

    public record AppReleaseResponse(string Version, DateTime ReleasedAt, IReadOnlyList<string> Changes);