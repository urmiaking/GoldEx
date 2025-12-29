namespace GoldEx.Client.Components.Services;

public sealed class HelpContext
{
    private string? _slug;

    public event Action? OnChanged;

    public string? Slug
    {
        get => _slug;
        set
        {
            if (_slug == value)
                return;

            _slug = value;
            OnChanged?.Invoke();
        }
    }
}