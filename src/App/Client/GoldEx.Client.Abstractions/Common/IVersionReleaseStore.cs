namespace GoldEx.Client.Abstractions.Common;

public interface IVersionReleaseStore
{
    Task<string?> GetLastSeenVersionAsync();
    Task SetLastSeenVersionAsync(string version);
}