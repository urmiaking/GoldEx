using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class BarcodeBuilder
{
    private readonly string _jsVersion = new Random().Next(1, 1000).ToString();

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "barcode-builder";
        base.OnInitialized();
    }
}