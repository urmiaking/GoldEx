using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class BarcodeBuilder
{
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "barcode-builder";
        base.OnInitialized();
    }
}