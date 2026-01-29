namespace GoldEx.Client.Components.Utilities;

public static class AppVersion
{
    public static bool IsNewer(string current, string? lastSeen)
    {
        if (string.IsNullOrWhiteSpace(current))
            return false;

        if (string.IsNullOrWhiteSpace(lastSeen))
            return true;

        if (!Version.TryParse(lastSeen, out var last))
            return true;

        return new Version(current) > last;
    }
}
