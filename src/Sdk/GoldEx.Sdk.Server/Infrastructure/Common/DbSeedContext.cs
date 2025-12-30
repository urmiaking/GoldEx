namespace GoldEx.Sdk.Server.Infrastructure.Common;

public sealed class DbSeedContext
{
    private readonly Dictionary<string, object> _items = new();

    public void Set<T>(string key, T value) where T : notnull
        => _items[key] = value;

    public bool TryGet<T>(string key, out T value)
    {
        if (_items.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    public T GetRequired<T>(string key)
        => TryGet<T>(key, out var value)
            ? value
            : throw new InvalidOperationException($"Seed context value '{key}' was not provided.");
}
