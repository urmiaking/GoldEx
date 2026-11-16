using GoldEx.Client.Components.Services;
using GoldEx.Client.Components.Components;
using Microsoft.AspNetCore.Components;

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
    private static readonly WorkspaceTab<int>[] Tools =
    [
        new(0, "محاسبه‌گر", "calculator", "قیمت نهایی طلا با اجرت، سود و مالیات"),
        new(1, "سینی هوشمند", "package", "بررسی و مدیریت اجناس سینی پیش‌خوان"),
        new(2, "محاسبه معکوس", "history", "محاسبهٔ وزن طلا از مبلغ نهایی"),
        new(3, "تبدیل ارز", "transfer", "تبدیل مبلغ بین واحدهای ارزی"),
        new(4, "برابری طلا", "gem", "بررسی برابری ارزها و حباب طلا"),
        new(5, "جستجوگر پیشرفته", "search", "جستجوی موجودی با بودجه، وزن و مشخصات")
    ];
    private WorkspaceTab<int> CurrentTool => Tools[_activeTabIndex];

    private bool _hasRequestedTab;
    private string? _requestedTab;
    private IEnumerable<WorkspaceTab<int>> VisibleTools => User?.Identity?.IsAuthenticated == true ? Tools : Tools.Take(1);

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
    public override async ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        await base.DisposeAsync();
    }
}
