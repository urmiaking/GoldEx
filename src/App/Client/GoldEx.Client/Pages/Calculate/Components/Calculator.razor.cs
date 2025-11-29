using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    //[Inject] public IBrowserViewportService BrowserViewportService { get; set; } = default!;

    //private bool _isMobile = false;
    //private bool _isInitialized;
    //private readonly Guid _observerId = Guid.NewGuid();

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (firstRender)
    //    {
    //        await BrowserViewportService.SubscribeAsync(
    //            _observerId,
    //            async (eventArgs) => await OnBreakpointChanged(eventArgs),
    //            new ResizeOptions { ReportRate = 250, NotifyOnBreakpointOnly = true }
    //        );

    //        await Task.Yield();
    //        await UpdateTabTitle();
    //        _isInitialized = true;
    //    }
    //}

    //private async Task OnBreakpointChanged(BrowserViewportEventArgs eventArgs)
    //{
    //    if (_isInitialized)
    //    {
    //        await UpdateTabTitle();
    //        StateHasChanged();
    //    }
    //}

    //private async Task UpdateTabTitle()
    //{
    //    var breakpoint = await BrowserViewportService.GetCurrentBreakpointAsync();

    //    var wasMobile = _isMobile;
    //    _isMobile = await BrowserViewportService.IsBreakpointWithinReferenceSizeAsync(
    //        Breakpoint.MdAndDown, breakpoint);

    //    if (_isMobile != wasMobile)
    //    {
    //        StateHasChanged();
    //    }
    //}

    //public override async ValueTask DisposeAsync()
    //{
    //    await BrowserViewportService.UnsubscribeAsync(_observerId);
    //    await base.DisposeAsync();
    //}
}