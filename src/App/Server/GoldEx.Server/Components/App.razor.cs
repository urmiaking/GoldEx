using GoldEx.Client.Components.Services;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GoldEx.Server.Components;

public partial class App
{
    private GetLicenseResponse? _license;

    [Inject] private ILicenseService LicenceService { get; set; } = default!;
    [Inject] private LicenseState LicenseState { get; set; } = default!;
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    private IComponentRenderMode? RenderModeForPage => HttpContext.Request.Path.StartsWithSegments("/Account")
        ? null
        : GetRenderMode();

    private bool IsLoggedIn => HttpContext.User.Identity?.IsAuthenticated ?? false;

    private IComponentRenderMode GetRenderMode()
    {
        if (HttpContext.Request.Path.StartsWithSegments("/ssr"))
            return RenderMode.InteractiveServer;

        return new InteractiveAutoRenderMode(true);
    }

    protected override async Task OnInitializedAsync()
    {
        await GetLicenseAsync();
        await base.OnInitializedAsync();
    }

    private async Task GetLicenseAsync()
    {
        _license = await LicenceService.GetLicenseAsync();
        LicenseState.Set(_license);
    }
}