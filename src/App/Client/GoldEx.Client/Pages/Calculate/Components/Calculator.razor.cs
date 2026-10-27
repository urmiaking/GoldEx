using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string? ContainerClass { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool HideTabs { get; set; }
    [Parameter] [SupplyParameterFromQuery(Name = "tab")] public string? Tab { get; set; }
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private int _activeTabIndex;
    private sealed record CalculatorTool(int Index, string Title, string Description, string Icon);
    private static readonly CalculatorTool[] Tools =
    [
        new(0, "محاسبه‌گر", "قیمت نهایی طلا با اجرت، سود و مالیات", "calculator"),
        new(1, "سینی هوشمند", "بررسی و مدیریت اجناس سینی پیش‌خوان", "package"),
        new(2, "محاسبه معکوس", "محاسبهٔ وزن طلا از مبلغ نهایی", "history"),
        new(3, "تبدیل ارز", "تبدیل مبلغ بین واحدهای ارزی", "transfer"),
        new(4, "برابری طلا", "بررسی برابری ارزها و حباب طلا", "gem"),
        new(5, "جستجوگر پیشرفته", "جستجوی موجودی با بودجه، وزن و مشخصات", "search")
    ];
    private CalculatorTool CurrentTool => Tools[Math.Clamp(_activeTabIndex, 0, Tools.Length - 1)];

    [Inject] private IJSRuntime JS { get; set; } = default!;
    private readonly string _tabsId = $"gex-calculator-{Guid.NewGuid():N}";
    private ElementReference _workspace;
    private IJSObjectReference? _tabsModule, _tabsBinding;
    private bool _bindingTabs, _hasRequestedTab;
    private string? _requestedTab;
    private IEnumerable<CalculatorTool> VisibleTools => User?.Identity?.IsAuthenticated == true ? Tools : Tools.Take(1);
    private string TabId(int index) => $"{_tabsId}-tab-{index}";
    private string PanelId(int index) => $"{_tabsId}-panel-{index}";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_hasRequestedTab && string.Equals(Tab, _requestedTab, StringComparison.OrdinalIgnoreCase)) return;
        _requestedTab = Tab;
        _hasRequestedTab = true;
        var trayRequested = string.Equals(Tab, "smart-tray", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(Tab, "tray", StringComparison.OrdinalIgnoreCase);
        SetHelpContext(trayRequested && User?.Identity?.IsAuthenticated == true ? 1 : 0);
    }
    private void SelectTool(int index)
    {
        if (index < 0 || index >= Tools.Length || (index > 0 && User?.Identity?.IsAuthenticated != true)) return;
        if (index != _activeTabIndex) SetHelpContext(index);
    }
    private void SetHelpContext(int index)
    {
        _activeTabIndex = index;
        HelpContext.Slug = "calculator-video";
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (HideTabs || _tabsBinding is not null || _bindingTabs || IsDisposed) return;
        _bindingTabs = true;
        try
        {
            _tabsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/GoldEx.Client.Components/js/calculator-tabs.js");
            _tabsBinding = await _tabsModule.InvokeAsync<IJSObjectReference>("bind", _workspace);
        }
        catch (JSDisconnectedException) { }
        finally { _bindingTabs = false; }
    }
    public override async ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        try
        {
            if (_tabsBinding is not null) { await _tabsBinding.InvokeVoidAsync("dispose"); await _tabsBinding.DisposeAsync(); }
            if (_tabsModule is not null) await _tabsModule.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        await base.DisposeAsync();
    }
}
