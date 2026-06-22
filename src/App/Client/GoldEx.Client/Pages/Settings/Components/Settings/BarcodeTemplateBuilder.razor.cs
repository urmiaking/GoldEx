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
    private BarcodePositionItemVm? _selectedItem;
    private bool _isLoading;
    private double _zoomLevel = 1.0;
    private int _activeMobileTab = 1; // 0 = Items, 1 = Preview, 2 = Properties

    private const int PreviewWidth = 600;  // Fixed preview area reference width
    private const int PreviewHeight = 400; // Fixed preview area reference height

    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        _isLoading = true;

        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                _settings = BarcodePrintSettingsVm.CreateFrom(response);
                _selectedItem = null;
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
                AddSuccessToast("تنظیمات قالب بارکد با موفقیت ذخیره شد");
                _isLoading = false;
                return Task.CompletedTask;
            },
            onFailure: () =>
            {
                _isLoading = false;
                return Task.CompletedTask;
            });
    }

    private BarcodePositionItemVm? GetActiveItem(BarcodePrintableItem itemType)
    {
        return _settings.PositionItems.FirstOrDefault(x => x.ItemType == itemType);
    }

    private void ToggleItemActive(BarcodePrintableItem itemType)
    {
        var existing = GetActiveItem(itemType);
        if (existing != null)
        {
            _settings.RemoveItem(existing);
            if (_selectedItem == existing)
            {
                _selectedItem = null;
            }
            _activeMobileTab = 0; // Stay on items list
            AddInfoToast($"{existing.GetLabel()} غیرفعال شد");
        }
        else
        {
            var defaultPosition = BarcodePosition.TopLeft;
            var order = _settings.GetItemsForPosition(defaultPosition).Count;

            var newItem = new BarcodePositionItemVm
            {
                ItemType = itemType,
                Position = defaultPosition,
                Order = order,
                IsVisible = true,
                FontSize = itemType == BarcodePrintableItem.Barcode ? 7.0 : 8.0,
                ItemSpacing = 0.5,
                IsInToolbox = false
            };

            if (itemType == BarcodePrintableItem.Barcode)
            {
                newItem.BarcodeWidth = 22.0;
                newItem.BarcodeHeight = 8.0;
                newItem.BarcodeDisplayValue = true;
                newItem.BarcodeFontSize = 7.0;
                newItem.BarcodeMargin = 0.0;
                newItem.BarWidthMultiplier = 2;
            }

            _settings.AddItem(newItem);
            _selectedItem = newItem;
            _activeMobileTab = 2; // Auto switch to properties tab on mobile
            AddSuccessToast($"{newItem.GetLabel()} فعال شد");
        }
        StateHasChanged();
    }

    private void SelectItem(BarcodePositionItemVm? item)
    {
        _selectedItem = item;
        _activeMobileTab = 2; // Auto switch to properties tab on mobile
        StateHasChanged();
    }

    private void ChangeItemPosition(BarcodePositionItemVm item, BarcodePosition newPosition)
    {
        if (item.Position == newPosition) return;

        // Save old position to re-order remaining items
        var oldPosition = item.Position;

        // Shift item to new position
        item.Position = newPosition;
        item.Order = _settings.GetItemsForPosition(newPosition).Count;

        // Re-order remaining items in old position
        ReorderPositionItems(oldPosition);
        ReorderPositionItems(newPosition);

        StateHasChanged();
    }

    private void MoveItemOrder(BarcodePositionItemVm item, bool moveUp)
    {
        var positionItems = _settings.GetItemsForPosition(item.Position);
        var index = positionItems.IndexOf(item);
        if (index == -1) return;

        if (moveUp && index > 0)
        {
            var prev = positionItems[index - 1];
            (item.Order, prev.Order) = (prev.Order, item.Order);
        }
        else if (!moveUp && index < positionItems.Count - 1)
        {
            var next = positionItems[index + 1];
            (item.Order, next.Order) = (next.Order, item.Order);
        }

        // Re-evaluate list order
        ReorderPositionItems(item.Position);
        StateHasChanged();
    }

    private void ReorderPositionItems(BarcodePosition position)
    {
        var items = _settings.PositionItems
            .Where(x => x.Position == position)
            .OrderBy(x => x.Order)
            .ToList();

        for (int i = 0; i < items.Count; i++)
        {
            items[i].Order = i;
        }
    }

    private void AdjustZoom(double delta)
    {
        _zoomLevel = Math.Clamp(_zoomLevel + delta, 0.5, 4.0);
        StateHasChanged();
    }

    private void ResetZoom()
    {
        var scaleX = (double)PreviewWidth / _settings.LabelWidth;
        var scaleY = (double)PreviewHeight / _settings.LabelHeight;
        _zoomLevel = Math.Min(Math.Min(scaleX, scaleY), 3.0);
        if (_zoomLevel < 1.0) _zoomLevel = 1.0;
        StateHasChanged();
    }

    private string GetPositionLabel(BarcodePosition position)
    {
        return position switch
        {
            BarcodePosition.TopLeft => "چپ - بالا",
            BarcodePosition.BottomLeft => "چپ - پایین",
            BarcodePosition.TopRight => "راست - بالا",
            BarcodePosition.BottomRight => "راست - پایین",
            _ => string.Empty
        };
    }

    private async Task PrintTestBarcode()
    {
        var testData = new
        {
            barcode = "10206610",
            productName = "گوشواره طلا",
            weight = "W: 3.258G",
            wage = "F: 1,150,000 TMN"
        };

        var settingsForJs = new
        {
            labelWidth = _settings.LabelWidth,
            labelHeight = _settings.LabelHeight,
            tailWidth = _settings.TailWidth,
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
                        margin = x.BarcodeMargin,
                        barWidthMultiplier = x.BarWidthMultiplier
                    }
                    : null
            }).ToArray()
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", settingsForJs, testData);
    }

    private void SetMobileTab(int tabIndex)
    {
        _activeMobileTab = tabIndex;
        StateHasChanged();
    }

    private string GetTabClass(int tabIndex)
    {
        return _activeMobileTab == tabIndex ? "mobile-active-tab" : "mobile-inactive-tab";
    }
}