using System.Text.Json;

namespace GoldEx.Client.Components.Components.Dialogs;

public static class EditorDraft
{
    // UI drafts must not mutate the displayed customer or nested accounts on cancel.
    public static T Copy<T>(T model) where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(model))
        ?? throw new InvalidOperationException("Unable to copy editor data.");
}
