using GoldEx.Client.Components.Services;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Components;

public partial class App
{
    private GetLicenseResponse? _license;

    [Inject] private ILicenseService LicenceService { get; set; } = default!;
    [Inject] private LicenseState LicenseState { get; set; } = default!;
    [Inject] private IWebHostEnvironment Env { get; set; } = default!;
    [Inject] private IStoreContext StoreContext { get; set; } = default!;
    [Inject] private GoldExDbContext DbContext { get; set; } = default!;
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    private string SplashTitle { get; set; } = "GoldEx";
    private string? SplashLogoUrl { get; set; }
    private bool IsStoreTitle { get; set; }

    private bool IsVitrineRoute()
    {
        var path = HttpContext.Request.Path.Value ?? "";
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return false;

        var firstSegment = segments[0];
        return !ClientRoutes.Vitrine.IsReservedSegment(firstSegment);
    }

    private IComponentRenderMode? RenderModeForPage => HttpContext.Request.Path.StartsWithSegments("/Account")
        ? null
        : GetRenderMode();

    private bool IsLoggedIn => HttpContext.User.Identity?.IsAuthenticated ?? false;

    private IComponentRenderMode GetRenderMode()
    {
        if (HttpContext.Request.Path.StartsWithSegments("/ssr"))
            return RenderMode.InteractiveServer;

        if (IsVitrineRoute())
            return new InteractiveWebAssemblyRenderMode(prerender: true);

        return new InteractiveWebAssemblyRenderMode(prerender: false);
    }

    /// <summary>
    /// Shows the loading splash screen only for admin/internal app when prerendering is disabled.
    /// Vitrine routes are prerendered and never show a splash screen.
    /// </summary>
    private bool ShowSplash
    {
        get
        {
            if (IsVitrineRoute())
                return false;

            return RenderModeForPage switch
            {
                InteractiveWebAssemblyRenderMode { Prerender: false } => true,
                InteractiveAutoRenderMode { Prerender: false } => true,
                _ => false
            };
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetLicenseAsync();
        if (!IsVitrineRoute())
        {
            await LoadSplashTitleAsync();
        }
        await base.OnInitializedAsync();
    }

    private async Task LoadSplashTitleAsync()
    {
        if (IsLoggedIn && StoreContext.StoreId.HasValue)
        {
            var storeId = new StoreId(StoreContext.StoreId.Value);
            var store = await DbContext.Set<Store>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store is not null)
            {
                if (!string.IsNullOrWhiteSpace(store.Name))
                {
                    SplashTitle = store.Name;
                    IsStoreTitle = true;
                }

                if (!string.IsNullOrWhiteSpace(store.LogoUrl))
                {
                    SplashLogoUrl = store.LogoUrl;
                }
            }
        }
        else
        {
            var path = HttpContext.Request.Path.Value ?? "";
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && !segments[0].StartsWith("_") && !segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) && !segments[0].Equals("Account", StringComparison.OrdinalIgnoreCase))
            {
                var slug = segments[0].ToLowerInvariant();
                var store = await DbContext.Set<Store>()
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug);

                if (store != null)
                {
                    if (!string.IsNullOrWhiteSpace(store.Name))
                    {
                        SplashTitle = store.Name;
                        IsStoreTitle = true;
                    }

                    if (!string.IsNullOrWhiteSpace(store.LogoUrl))
                    {
                        SplashLogoUrl = store.LogoUrl;
                    }
                }
            }
        }
    }

    private async Task GetLicenseAsync()
    {
        _license = await LicenceService.GetLicenseAsync();
        LicenseState.Set(_license);
    }
}