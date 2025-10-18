using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace GoldEx.Client.Pages.Home;

public partial class Index
{
    [Inject] public IBrowserViewportService BrowserViewportService { get; set; } = default!;

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];

    private Position _tabPosition = Position.Top;
    private string _contentClass = "mt-5";
    private bool _isMobile = true;
    private bool _isInitialized;
    private readonly Guid _observerId = Guid.NewGuid();

    private string PriceBoardClass => User?.Identity?.IsAuthenticated ?? false ? "responsive-table" : "responsive-table-login";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await BrowserViewportService.SubscribeAsync(
                _observerId,
                async (eventArgs) => await OnBreakpointChanged(eventArgs),
                new ResizeOptions { ReportRate = 250, NotifyOnBreakpointOnly = true }
            );

            // Wait for next render cycle to allow resize observer to measure  
            await Task.Yield();
            await UpdateTabPosition();
            _isInitialized = true;
        }
    }

    private async Task OnBreakpointChanged(BrowserViewportEventArgs eventArgs)
    {
        if (_isInitialized)
        {
            await UpdateTabPosition();
            StateHasChanged();
        }
    }

    private async Task UpdateTabPosition()
    {
        var breakpoint = await BrowserViewportService.GetCurrentBreakpointAsync();

        var wasMobile = _isMobile;
        _isMobile = await BrowserViewportService.IsBreakpointWithinReferenceSizeAsync(
            Breakpoint.MdAndDown, breakpoint);

        if (_isMobile)
        {
            _tabPosition = Position.Top;
            _contentClass = "mt-5";
        }
        else
        {
            _tabPosition = Position.Top;
            _contentClass = "mt-5";
        }

        // Only trigger state change if position actually changed  
        if (wasMobile != _isMobile)
        {
            StateHasChanged();
        }

        StateHasChanged();
    }

    public override async ValueTask DisposeAsync()
    {
        await BrowserViewportService.UnsubscribeAsync(_observerId);
        await base.DisposeAsync();
    }
}