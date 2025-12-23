using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class ProductList
{
    private decimal _gramPrice;
    private GetPriceUnitTitleResponse? _gramPriceUnit;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private GetBarcodePrintSettingsResponse? _barcodeSettings;

    private object SettingsForJs => new
    {
        labelWidth = _barcodeSettings?.LabelWidth,
        labelHeight = _barcodeSettings?.LabelHeight,
        marginTop = _barcodeSettings?.MarginTop,
        marginRight = _barcodeSettings?.MarginRight,
        marginBottom = _barcodeSettings?.MarginBottom,
        marginLeft = _barcodeSettings?.MarginLeft,
        paddingTop = _barcodeSettings?.PaddingTop,
        paddingRight = _barcodeSettings?.PaddingRight,
        paddingBottom = _barcodeSettings?.PaddingBottom,
        paddingLeft = _barcodeSettings?.PaddingLeft,
        positionItems = _barcodeSettings?.PositionItems.Select(x => new
        {
            position = x.Position.ToString(),
            itemType = x.ItemType.ToString(),
            order = x.Order,
            isVisible = x.IsVisible,
            fontSize = x.FontSize,
            itemSpacing = x.ItemSpacing,
            barcodeSettings = x.BarcodeSettings != null
                ? new
                {
                    width = x.BarcodeSettings.Width,
                    height = x.BarcodeSettings.Height,
                    displayValue = x.BarcodeSettings.DisplayValue,
                    fontSize = x.BarcodeSettings.FontSize,
                    margin = x.BarcodeSettings.Margin
                }
                : null
        }).ToArray()
    };

    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }
    [Parameter, EditorRequired] public InventoryEntryVm Model { get; set; } = null!;
    [Parameter] public EventCallback<decimal> GramPriceChanged { get; set; }

    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadGramPriceAsync();
        await LoadPriceUnitsAsync();
        await LoadBarcodeSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(GoldUnitType.Gram, null, false, ct),
            afterSend: async response =>
            {
                decimal.TryParse(response?.Value, out var gramPrice);
                _gramPrice = gramPrice;
                await GramPriceChanged.InvokeAsync(_gramPrice);
            });
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;
                _gramPriceUnit = _priceUnits.FirstOrDefault(pu => pu.IsGoldBased);
            });
    }

    private async Task LoadBarcodeSettingsAsync()
    {
        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, token) => s.GetAsync(token),
            afterSend: response => _barcodeSettings = response,
            createScope: true
        );
    }

    private async Task OnGramPriceChanged(decimal value)
    {
        _gramPrice = value;

        foreach (var item in Model.ProductItems)
        {
            item.GramPrice = 1;
            item.RecalculateAmounts();
        }

        await GramPriceChanged.InvokeAsync(value);
    }

    private async Task PrintBarcode(ProductItemVm item)
    {
        if (_barcodeSettings is null)
        {
            AddErrorToast("تنظیمات چاپ بارکد لود نشده است");
            return;
        }

        var data = new
        {
            barcode = item.Product.Barcode ?? "",
            productName = item.Product.Name ?? "",
            weight = $"W: {item.TotalWeight:G29}{(item.Product.GoldUnitType == GoldUnitType.Gram ? "G" : "M")}",
            wage = "F: " + item.Product.WageType switch
            {
                WageType.Fixed => $"{item.Product.Wage?.ToCurrencyFormat()} {item.Product.WagePriceUnitTitle}",
                WageType.Percent => $"{item.Product.Wage:G29}%",
                _ => "---"
            }
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
    }

    private async Task RemoveItem(ProductItemVm context)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف جنس {context.Product.Name} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.ProductItems.Remove(context);
            await ReleaseReservedBarcode(context);
            StateHasChanged();
        }
    }

    private async Task AddItem()
    {
        if (PriceUnit is null)
            return;

        var model = ProductItemVm.CreateDefaultInstance();

        model.GramPrice = 1;
        model.InvoiceType = InvoiceType.Purchase;
        model.CostPriceUnitId = _gramPriceUnit?.Id;
        model.CostPriceUnitTitle = _gramPriceUnit?.Title;

        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _gramPriceUnit }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("افزودن جنس جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm productItem })
        {
            await ReserveBarcode(productItem);

            productItem.RecalculateAmounts();
            productItem.Index = GetLastItemIndexNumber() + 1;
            Model.ProductItems.Add(productItem);

            StateHasChanged();
        }
    }

    private async Task UploadExcel()
    {
        var dialog = await DialogService.ShowAsync<ExcelUploadDialog>("آپلود فایل اکسل", _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<ProductItemVm> items })
        {
            foreach (var item in items)
            {
                item.GramPrice = 1;
                item.RecalculateAmounts();
                item.Index = GetLastItemIndexNumber() + 1;
                Model.ProductItems.Add(item);
            }
            StateHasChanged();
        }
    }

    private void SyncWithApi()
    {
        AddInfoToast("این قابلیت به زودی فعال خواهد شد");
    }

    public int GetLastItemIndexNumber()
    {
        return Model.ProductItems.Count > 0 ? Model.ProductItems.Max(i => i.Index) : 0;
    }

    private async Task ReserveBarcode(ProductItemVm item)
    {
        if (item.Product.Id.HasValue)
            return;

        if (!string.IsNullOrWhiteSpace(item.Product.Barcode))
            return;

        var request = new IssueNextBarcodeRequest(BarcodeType.Product, item.Product.ProductType, item.Product.ProductCategoryId, null);

        await SendRequestAsync<IBarcodeReservationService, IssueNextBarcodeResponse>(
            action: (svc, ct) => svc.IssueNextAsync(request, ct),
            afterSend: resp =>
            {
                item.Product.Barcode = resp.Barcode;
            });
    }

    private async Task ReleaseReservedBarcode(ProductItemVm item)
    {
        if (item.Product.Id.HasValue)
            return;

        if (string.IsNullOrWhiteSpace(item.Product.Barcode))
            return;

        await SendRequestAsync<IBarcodeReservationService>(
            action: (svc, ct) => svc.ReleaseAsync(BarcodeType.Product, item.Product.Barcode, ct));
    }

    private async Task EditItem(ProductItemVm context)
    {
        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, context },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _gramPriceUnit }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm resultItem })
        {
            context.UpdateFrom(resultItem);

            await ReserveBarcode(context);

            StateHasChanged();
        }
    }
}