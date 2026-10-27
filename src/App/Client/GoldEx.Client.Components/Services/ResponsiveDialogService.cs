using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Services;

public sealed record ResponsiveDialogResult(bool Canceled, object? Data = null);

public sealed class ResponsiveDialogSession
{
    private readonly ResponsiveDialogService _owner;
    private readonly TaskCompletionSource<ResponsiveDialogResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal ResponsiveDialogSession(ResponsiveDialogService owner, Type component, string title, Dictionary<string, object> parameters, bool wide)
        => (_owner, Component, Title, Parameters, Wide) = (owner, component, title, parameters, wide);

    public Type Component { get; }
    public string Key { get; } = Guid.NewGuid().ToString("N");
    public string Title { get; }
    public IDictionary<string, object> Parameters { get; }
    public bool Wide { get; }
    public bool IsBusy { get; private set; }
    public bool IsClosed => _completion.Task.IsCompleted;
    public Task<ResponsiveDialogResult> Result => _completion.Task;
    public void SetBusy(bool busy) { if (IsClosed || IsBusy == busy) return; IsBusy = busy; _owner.NotifyChanged(); }
    public void Close(object? data = null) => Complete(new(false, data));
    public void Cancel() { if (!IsBusy) Complete(new(true)); }
    internal void ForceCancel() => Complete(new(true));
    private void Complete(ResponsiveDialogResult result)
    {
        if (!_completion.TrySetResult(result)) return;
        _owner.Remove(this);
    }
}

public sealed class ResponsiveDialogService
{
    private readonly List<ResponsiveDialogSession> _sessions = [];
    public IReadOnlyList<ResponsiveDialogSession> Sessions => _sessions.AsReadOnly();
    public event Action? Changed;

    public ResponsiveDialogSession Show<TComponent>(string title, Dictionary<string, object>? parameters = null, bool wide = false)
        where TComponent : IComponent
    {
        var session = new ResponsiveDialogSession(this, typeof(TComponent), title, new(parameters ?? []), wide);
        _sessions.Add(session);
        NotifyChanged();
        return session;
    }

    public void CancelAll() { foreach (var session in _sessions.ToArray().Reverse()) session.ForceCancel(); }
    internal void Remove(ResponsiveDialogSession session)
    {
        var index = _sessions.IndexOf(session);
        if (index < 0) return;
        foreach (var child in _sessions.Skip(index + 1).Reverse().ToArray()) child.ForceCancel();
        _sessions.Remove(session);
        NotifyChanged();
    }
    internal void NotifyChanged() => Changed?.Invoke();
}
