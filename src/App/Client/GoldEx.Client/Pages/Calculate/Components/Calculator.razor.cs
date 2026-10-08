using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string? ContainerClass { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool HideTabs { get; set; }
    [Parameter] [SupplyParameterFromQuery(Name = "tab")] public string? Tab { get; set; }
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private int _activeTabIndex;

    protected override void OnInitialized()
    {
        if (string.Equals(Tab, "smart-tray", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Tab, "tray", StringComparison.OrdinalIgnoreCase))
        {
            SetHelpContext(1);
        }
        else
        {
            SetHelpContext(0);
        }
        base.OnInitialized();
    }

    private void SetHelpContext(int index)
    {
        _activeTabIndex = index;
        switch (index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                HelpContext.Slug = "calculator-video";
                break;
        }
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }
}