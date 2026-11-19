namespace GoldEx.Client.Components.Components;

public sealed record WorkspaceTab<TKey>(TKey Key, string Title, string Icon, string? Description = null)
    where TKey : notnull;
