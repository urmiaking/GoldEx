using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Settings;

public partial class BarcodeTemplateBuilder
{
    private BarcodePrintSettingsVm _settings = new();
    private List<BarcodePositionItemVm> _allItems = new();
    private bool _isLoading = false;
    private double _zoomLevel = 1.0;

    private const int PREVIEW_WIDTH = 600;  // عرض ثابت پیش‌نمایش
    private const int PREVIEW_HEIGHT = 400; // ارتفاع ثابت پیش‌نمایش
    private const int MIN_ZONE_SIZE = 120;  // حداقل اندازه zone

    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
        InitializeToolbox();
    }

    private void InitializeToolbox()
    {
        // فقط اگر آیتم‌های Toolbox وجود نداشته باشن اضافه کن
        if (!_allItems.Any(x => x.IsInToolbox))
        {
            var toolboxItems = new List<BarcodePositionItemVm>
            {
                BarcodePositionItemVm.CreateToolboxItem(BarcodePrintableItem.Barcode),
                BarcodePositionItemVm.CreateToolboxItem(BarcodePrintableItem.ProductName),
                BarcodePositionItemVm.CreateToolboxItem(BarcodePrintableItem.Weight),
                BarcodePositionItemVm.CreateToolboxItem(BarcodePrintableItem.Wage)
            };

            _allItems.AddRange(toolboxItems);
        }
    }

    private async Task LoadSettings()
    {
        _isLoading = true;

        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                _settings = BarcodePrintSettingsVm.CreateFrom(response);
                _allItems.Clear();
                InitializeToolbox();
                _allItems.AddRange(_settings.PositionItems);
                _isLoading = false;
            },
            onFailure: () =>
            {
                _isLoading = false;
            });
    }

    private async Task SaveSettings()
    {
        _isLoading = true;
        await SendRequestAsync<IBarcodePrintSettingsService>(
            action: (s, ct) => s.UpdateAsync(_settings.ToRequest(), ct),
            afterSend: () =>
            {
                AddSuccessToast("تنظیمات با موفقیت ذخیره شد");
                _isLoading = false;
                return Task.CompletedTask;
            },
            onFailure: () =>
            {
                _isLoading = false;
                return Task.CompletedTask;
            });
    }

    private bool ItemSelector(BarcodePositionItemVm item, string dropzone)
    {
        if (dropzone == "Toolbox")
            return item.IsInToolbox;

        if (Enum.TryParse<BarcodePosition>(dropzone, out var position))
            return !item.IsInToolbox && item.Position == position;

        return false;
    }

    private Func<BarcodePositionItemVm, string, bool> CanDrop => (item, dropzone) =>
    {
        // همیشه می‌توان به Toolbox برگرداند
        if (dropzone == "Toolbox")
            return true;

        // اگر از Toolbox میاد، چک کن که تکراری نباشه
        if (item.IsInToolbox && Enum.TryParse<BarcodePosition>(dropzone, out var targetPosition))
        {
            return !_allItems.Any(x => !x.IsInToolbox &&
                                       x.Position == targetPosition &&
                                       x.ItemType == item.ItemType);
        }

        return true;
    };

    private void OnItemDropped(MudItemDropInfo<BarcodePositionItemVm> dropInfo)
    {
        if (dropInfo.Item is null) return;

        var item = dropInfo.Item;

        // برگشت به Toolbox
        if (dropInfo.DropzoneIdentifier == "Toolbox")
        {
            if (!item.IsInToolbox)
            {
                _settings.RemoveItem(item);
                item.IsInToolbox = true;
                AddInfoToast($"{item.GetLabel()} به جعبه ابزار برگشت");
            }
            return;
        }

        // انتقال به موقعیت
        if (Enum.TryParse<BarcodePosition>(dropInfo.DropzoneIdentifier, out var position))
        {
            if (item.IsInToolbox)
            {
                // از Toolbox به موقعیت
                var exists = _allItems.Any(x => !x.IsInToolbox &&
                                                x.Position == position &&
                                                x.ItemType == item.ItemType);

                if (exists)
                {
                    AddWarningToast("این آیتم قبلاً در این موقعیت وجود دارد");
                    return;
                }

                var newItem = new BarcodePositionItemVm
                {
                    ItemType = item.ItemType,
                    Position = position,
                    Order = _settings.GetItemsForPosition(position).Count,
                    IsVisible = true,
                    FontSize = 12,
                    ItemSpacing = 5,
                    IsInToolbox = false
                };

                _settings.AddItem(newItem);
                _allItems.Add(newItem);

                AddSuccessToast($"{newItem.GetLabel()} به {GetPositionLabel(position)} اضافه شد");
            }
            else
            {
                // تغییر موقعیت
                if (item.Position != position)
                {
                    var exists = _allItems.Any(x => !x.IsInToolbox &&
                                                    x.Position == position &&
                                                    x.ItemType == item.ItemType &&
                                                    x != item);

                    if (exists)
                    {
                        AddWarningToast("این آیتم قبلاً در این موقعیت وجود دارد");
                        return;
                    }

                    item.Position = position;
                    item.Order = dropInfo.IndexInZone;
                    AddInfoToast($"{item.GetLabel()} به {GetPositionLabel(position)} منتقل شد");
                }
                else
                {
                    // تغییر ترتیب در همان موقعیت
                    item.Order = dropInfo.IndexInZone;
                }
            }

            StateHasChanged();
        }
    }

    private void RemoveItem(BarcodePositionItemVm item)
    {
        if (item.IsInToolbox) return; // نمی‌شه آیتم Toolbox رو حذف کرد

        _settings.RemoveItem(item);
        _allItems.Remove(item);
        AddInfoToast($"{item.GetLabel()} حذف شد");
        StateHasChanged();
    }

    private async Task ConfigureItem(BarcodePositionItemVm item)
    {
        var parameters = new DialogParameters<BarcodeItemConfigDialog>
        {
            { x => x.Item, item }
        };

        var dialog = await DialogService.ShowAsync<BarcodeItemConfigDialog>("تنظیمات آیتم", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            StateHasChanged();
        }
    }

    private void AdjustZoom(double delta)
    {
        _zoomLevel = Math.Clamp(_zoomLevel + delta, 0.5, 2.0);
        StateHasChanged();
    }

    private void ResetZoom()
    {
        // محاسبه زوم خودکار
        var scaleX = (double)PREVIEW_WIDTH / _settings.LabelWidth;
        var scaleY = (double)PREVIEW_HEIGHT / _settings.LabelHeight;
        _zoomLevel = Math.Min(Math.Min(scaleX, scaleY), 2.0);
        StateHasChanged();
    }

    private string GetPreviewWrapperStyle()
    {
        // محاسبه scale اولیه
        var scaleX = (double)PREVIEW_WIDTH / _settings.LabelWidth;
        var scaleY = (double)PREVIEW_HEIGHT / _settings.LabelHeight;
        var autoScale = Math.Min(scaleX, scaleY);
        autoScale = Math.Min(autoScale, 2.0);

        // اعمال زوم دستی
        var finalScale = autoScale * _zoomLevel;

        return $"width: {PREVIEW_WIDTH}px; height: {PREVIEW_HEIGHT}px; --preview-scale: {finalScale.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)};";
    }

    private string GetLabelContainerStyle()
    {
        return "display: flex; justify-content: center; align-items: center; min-height: 400px; background: #f5f5f5; border-radius: 8px; padding: 20px;";
    }

    private string GetLabelStyle()
    {
        return $"width: {_settings.LabelWidth}px; height: {_settings.LabelHeight}px; position: relative; background: white; border: 2px dashed #ccc; border-radius: 4px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);";
    }

    private string GetPositionStyle(BarcodePosition position)
    {
        return position switch
        {
            BarcodePosition.TopLeft => $"top: {_settings.PaddingTop}px; left: {_settings.PaddingLeft}px;",
            BarcodePosition.TopRight => $"top: {_settings.PaddingTop}px; right: {_settings.PaddingRight}px;",
            BarcodePosition.BottomLeft => $"bottom: {_settings.PaddingBottom}px; left: {_settings.PaddingLeft}px;",
            BarcodePosition.BottomRight => $"bottom: {_settings.PaddingBottom}px; right: {_settings.PaddingRight}px;",
            _ => string.Empty
        };
    }

    private string GetPositionLabel(BarcodePosition position)
    {
        return position switch
        {
            BarcodePosition.TopLeft => "گوشه بالا راست",
            BarcodePosition.TopRight => "گوشه بالا چپ",
            BarcodePosition.BottomLeft => "گوشه پایین راست",
            BarcodePosition.BottomRight => "گوشه پایین چپ",
            _ => string.Empty
        };
    }

    private string GetPositionWrapperStyle(BarcodePosition position)
    {
        // محاسبه حداکثر عرض/ارتفاع برای هر موقعیت
        var maxWidth = Math.Max((_settings.LabelWidth / 2) - (_settings.PaddingLeft + _settings.PaddingRight) / 2, MIN_ZONE_SIZE);
        var maxHeight = Math.Max((_settings.LabelHeight / 2) - (_settings.PaddingTop + _settings.PaddingBottom) / 2, MIN_ZONE_SIZE);

        var baseStyle = $"max-width: {maxWidth}px; min-width: {MIN_ZONE_SIZE}px;";

        return position switch
        {
            BarcodePosition.TopLeft => 
                $"{baseStyle} top: {_settings.PaddingTop}px; left: {_settings.PaddingLeft}px;",
            
            BarcodePosition.TopRight => 
                $"{baseStyle} top: {_settings.PaddingTop}px; right: {_settings.PaddingRight}px;",
            
            BarcodePosition.BottomLeft => 
                $"{baseStyle} bottom: {_settings.PaddingBottom}px; left: {_settings.PaddingLeft}px;",
            
            BarcodePosition.BottomRight => 
                $"{baseStyle} bottom: {_settings.PaddingBottom}px; right: {_settings.PaddingRight}px;",
            
            _ => baseStyle
        };
    }

    private async Task PrintTestBarcode()
    {
        var testData = new
        {
            barcode = "1234567890",
            productName = "گوشواره طلا",
            weight = "3.5g",
            wage = "150,000 ریال"
        };

        var settingsForJs = new
        {
            labelWidth = _settings.LabelWidth,
            labelHeight = _settings.LabelHeight,
            marginTop = _settings.MarginTop,
            marginRight = _settings.MarginRight,
            marginBottom = _settings.MarginBottom,
            marginLeft = _settings.MarginLeft,
            paddingTop = _settings.PaddingTop,
            paddingRight = _settings.PaddingRight,
            paddingBottom = _settings.PaddingBottom,
            paddingLeft = _settings.PaddingLeft,
            positionItems = _settings.PositionItems.Select(x => new
            {
                position = x.Position.ToString(),
                itemType = x.ItemType.ToString(),
                order = x.Order,
                isVisible = x.IsVisible,
                fontSize = x.FontSize,
                itemSpacing = x.ItemSpacing,
                barcodeSettings = x.ItemType == BarcodePrintableItem.Barcode
                    ? new
                    {
                        width = x.BarcodeWidth,
                        height = x.BarcodeHeight,
                        displayValue = x.BarcodeDisplayValue,
                        fontSize = x.BarcodeFontSize,
                        margin = x.BarcodeMargin
                    }
                    : null
            }).ToArray()
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", settingsForJs, testData);
    }
}