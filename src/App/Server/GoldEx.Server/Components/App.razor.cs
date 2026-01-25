using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GoldEx.Server.Components;

public partial class App
{
    [Inject] private ISettingService SettingService { get; set; } = default!;
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    private IComponentRenderMode? RenderModeForPage => HttpContext.Request.Path.StartsWithSegments("/Account")
        ? null
        : GetRenderMode();

    private IComponentRenderMode GetRenderMode()
    {
        if (HttpContext.Request.Path.StartsWithSegments("/ssr"))
            return RenderMode.InteractiveServer;

        return new InteractiveAutoRenderMode(true);
    }
}