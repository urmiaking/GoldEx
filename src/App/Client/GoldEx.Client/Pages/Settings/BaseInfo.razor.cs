using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings;

public partial class BaseInfo
{
    private SettingsVm _model = new();
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "settings-video";
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                if (response is not null)
                {
                    _model = SettingsVm.CreateFromRequest(response);
                }
                else
                {
                    AddErrorToast("فراخوانی تنظیمات با مشکل مواجه شد");
                }
            });
    }

    private async Task OnGallerySettingsSubmitted(EditContext context)
    {
        if (_model.IconFile is not null)
        {
            await using var stream = _model.IconFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024); // 5 MB
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _model.IconContent = memoryStream.ToArray();
        }

        await SendRequestAsync<ISettingService>(
            action: (s, ct) => s.UpdateAsync(_model.ToRequest(), ct));

        AddSuccessToast("تنظیمات گالری با موفقیت ذخیره شد");

        await LoadSettingsAsync();
        StateHasChanged();
    }

    public record ThemePresetItem(
        string Key,
        string Title,
        string Subtitle,
        string PrimaryColor,
        string AccentColor,
        string BackgroundColor,
        string SurfaceColor,
        bool IsDark);

    private static readonly IReadOnlyList<ThemePresetItem> ThemePresets =
    [
        new("royal-emerald", "زمرد سلطنتی کاخ", "سبز زمردی و طلای شامپاینی (پیش‌فرض)", "#0f342e", "#d5b267", "#fbf9f5", "#ffffff", false),
        new("persian-turquoise", "فیروزه نیشابور", "فیروزه‌ای اصیل و طلای ناب ۱۸ عیار", "#0d6e6e", "#dfb260", "#f4fbfb", "#ffffff", false),
        new("imperial-ruby", "یاقوت سرخ شاهانه", "زرشکی مخملی و طلای گرم", "#3b0f19", "#dfb260", "#faf6f6", "#ffffff", false),
        new("yemeni-agate", "عقیق یمنی و کهربا", "عقیق کهربایی گرم و طلای خالص", "#7c2d12", "#e5b85a", "#fdf8f5", "#ffffff", false),
        new("champagne-pearl", "مروارید و شامپاین", "عاجی مرواریدی و طلای عتیقه", "#2b2723", "#c99d52", "#faf8f5", "#ffffff", false),
        new("minimal-white", "مدرن مینیمال سفید", "سفید استودیو و طلای ۷۵۰", "#111827", "#d4af37", "#ffffff", "#fbfbfb", false),
        new("rose-gold", "رز گلد و بژ مخملی", "صورتی صدفی و رزگلد متالیک", "#2f1d24", "#d98c80", "#fcf9f9", "#ffffff", false)
    ];

    private void SelectPreset(ThemePresetItem preset)
    {
        _model.VitrineThemePreset = preset.Key;
        _model.VitrinePrimaryColor = null;
        _model.VitrineAccentColor = null;
        _model.VitrineBackgroundColor = null;
        _model.VitrineSurfaceColor = null;
        StateHasChanged();
    }

    private ThemePresetItem GetCurrentPreset()
    {
        return ThemePresets.FirstOrDefault(p => p.Key.Equals(_model.VitrineThemePreset, StringComparison.OrdinalIgnoreCase))
            ?? ThemePresets[0];
    }

    private string GetActivePrimaryColor()
    {
        if (!string.IsNullOrWhiteSpace(_model.VitrinePrimaryColor))
            return _model.VitrinePrimaryColor;
        return GetCurrentPreset().PrimaryColor;
    }

    private string GetActiveAccentColor()
    {
        if (!string.IsNullOrWhiteSpace(_model.VitrineAccentColor))
            return _model.VitrineAccentColor;
        return GetCurrentPreset().AccentColor;
    }

    private string GetActiveBackgroundColor()
    {
        if (!string.IsNullOrWhiteSpace(_model.VitrineBackgroundColor))
            return _model.VitrineBackgroundColor;
        return GetCurrentPreset().BackgroundColor;
    }

    private string GetActiveSurfaceColor()
    {
        if (!string.IsNullOrWhiteSpace(_model.VitrineSurfaceColor))
            return _model.VitrineSurfaceColor;
        return GetCurrentPreset().SurfaceColor;
    }

    private bool IsActiveDark()
    {
        return GetCurrentPreset().IsDark;
    }
}