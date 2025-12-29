using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class ReportDesigner
{
    [Parameter] public string Name { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? DisplayName { get; set; }

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "report-list";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }
}