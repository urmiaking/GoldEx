using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        SetHelpContext(0);
        base.OnInitialized();
    }

    private void SetHelpContext(int index)
    {
        switch (index)
        {
            case 0:
                HelpContext.Slug = "calculator-video";
                break;
            case 1:
                HelpContext.Slug = "calculator-video";
                break;
            case 2:
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