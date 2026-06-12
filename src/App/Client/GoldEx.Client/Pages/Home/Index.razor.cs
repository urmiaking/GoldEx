using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Components.Components;
using GoldEx.Client.Components.Utilities;
using GoldEx.Shared.DTOs.AppReleases;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Home;

public partial class Index
{
    [Inject] private IVersionReleaseStore ReleaseStore { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new ("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];

    private string PriceBoardClass => User?.Identity?.IsAuthenticated ?? false ? "responsive-table" : "responsive-table-login";

    private IReadOnlyList<AppReleaseResponse> _releases = [];
    private bool _releaseChecked;

    [Parameter, SupplyParameterFromQuery(Name = "tab")]
    public int? Tab { get; set; }

    private int _activePanelIndex;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Tab.HasValue)
        {
            int maxIndex = (User?.Identity?.IsAuthenticated == true) ? 5 : 1;
            _activePanelIndex = System.Math.Clamp(Tab.Value, 0, maxIndex);
        }
    }

    private void OnTabChanged(int index)
    {
        if (_activePanelIndex == index)
            return;

        _activePanelIndex = index;
        var newUri = Navigation.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["tab"] = index
        });
        Navigation.NavigateTo(newUri, replace: true);
    }

    private async Task LoadReleasesAsync()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            await SendRequestAsync<IAppReleaseService, List<AppReleaseResponse>>(
                action: (s, ct) => s.GetListAsync(ct),
                afterSend: response => _releases = response,
                createScope: true);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _releaseChecked)
            return;

        await LoadReleasesAsync();

        if (!_releases.Any())
            return; 

        _releaseChecked = true;

        var current = _releases
            .OrderByDescending(r => Version.Parse(r.Version))
            .First();

        string? lastSeen;

        try
        {
            lastSeen = await ReleaseStore.GetLastSeenVersionAsync();
        }
        catch
        {
            lastSeen = null;
        }

        if (!AppVersion.IsNewer(current.Version, lastSeen))
            return;

        try
        {
            await ShowReleaseDialogAsync(current);
            await ReleaseStore.SetLastSeenVersionAsync(current.Version);
        }
        catch
        {
            // Fail silently – user can see changelog manually
        }

        await base.OnAfterRenderAsync(firstRender);
    }


    private async Task ShowReleaseDialogAsync(AppReleaseResponse current)
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };

        var parameters = new DialogParameters<ReleaseHistory>
        {
            { x => x.AllReleases, _releases },
            { x => x.Current, current }
        };

        var dialog = await DialogService.ShowAsync<ReleaseHistory>("تغییرات نسخه جدید", parameters, dialogOptions);

        await dialog.Result;
    }
}