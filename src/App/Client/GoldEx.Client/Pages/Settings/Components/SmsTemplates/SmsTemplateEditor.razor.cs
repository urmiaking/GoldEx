using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.SmsTemplates;

public partial class SmsTemplateEditor
{
    [Parameter, EditorRequired] public SmsTemplateItemVm Model { get; set; }
    [Parameter, EditorRequired] public string[] Parameters { get; set; }
    [Parameter, EditorRequired] public SmsTemplateSubject Subject { get; set; }

    private readonly List<Color> _colors = [Color.Success, Color.Primary, Color.Error, Color.Info, Color.Tertiary];

    private Color GetRandomColor()
    {
        var rnd = new Random().Next(0, 5);

        return _colors[rnd];
    }

    private void OnParameterClicked(string parameter)
    {
        Model.Body += $" ({parameter})";
    }

    private string GetStatusLabel() => Model.IsActive ? "فعال" : "غیرفعال";
}