using GoldEx.Client.Components.Services;
using GoldEx.Server.Domain.SettingAggregate;
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
    private string? StoreLogoUrl { get; set; }
    private string? StoreName { get; set; }
    private bool IsStoreTitle { get; set; }

    private string VitrineThemePreset { get; set; } = "royal-emerald";
    private string? VitrinePrimaryColor { get; set; }
    private string? VitrineAccentColor { get; set; }
    private string? VitrineBackgroundColor { get; set; }
    private string? VitrineSurfaceColor { get; set; }

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
        await LoadStoreMetadataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadStoreMetadataAsync()
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
                    StoreName = store.Name;
                    IsStoreTitle = true;
                }

                if (!string.IsNullOrWhiteSpace(store.LogoUrl))
                {
                    SplashLogoUrl = store.LogoUrl;
                    StoreLogoUrl = store.LogoUrl;
                }
            }
        }
        else
        {
            var path = HttpContext.Request.Path.Value ?? "";
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && !ClientRoutes.Vitrine.IsReservedSegment(segments[0]))
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
                        StoreName = store.Name;
                        IsStoreTitle = true;
                    }

                    if (!string.IsNullOrWhiteSpace(store.LogoUrl))
                    {
                        SplashLogoUrl = store.LogoUrl;
                        StoreLogoUrl = store.LogoUrl;
                    }

                    var setting = await DbContext.Set<Setting>()
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(s => s.StoreId == store.Id);

                    if (setting != null)
                    {
                        VitrineThemePreset = setting.VitrineThemePreset ?? "royal-emerald";
                        VitrinePrimaryColor = setting.VitrinePrimaryColor;
                        VitrineAccentColor = setting.VitrineAccentColor;
                        VitrineBackgroundColor = setting.VitrineBackgroundColor;
                        VitrineSurfaceColor = setting.VitrineSurfaceColor;
                    }
                }
            }
        }
    }

    private string GetVitrineThemeColorMeta()
    {
        if (!string.IsNullOrWhiteSpace(VitrinePrimaryColor))
            return VitrinePrimaryColor;

        return (VitrineThemePreset?.ToLowerInvariant()) switch
        {
            "persian-turquoise" or "turquoise" => "#0d6e6e",
            "imperial-ruby" or "ruby" => "#3b0f19",
            "yemeni-agate" or "agate" or "amber" => "#7c2d12",
            "champagne-pearl" or "pearl" or "champagne" => "#2b2723",
            "minimal-white" or "minimal" => "#111827",
            "rose-gold" or "rosegold" => "#2f1d24",
            _ => "#0f342e" // royal-emerald
        };
    }

    private async Task GetLicenseAsync()
    {
        _license = await LicenceService.GetLicenseAsync();
        LicenseState.Set(_license);
    }
}