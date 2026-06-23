using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class CoinList
{
    [Parameter, EditorRequired] public GetPriceUnitResponse? PriceUnit { get; set; }
    [Parameter, EditorRequired] public InventoryEntryVm Model { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private GetBarcodePrintSettingsResponse? _barcodeSettings;

    private object SettingsForJs => new
    {
        labelWidth = _barcodeSettings?.LabelWidth,
        labelHeight = _barcodeSettings?.LabelHeight,
        tailWidth = _barcodeSettings?.TailWidth,
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
                    margin = x.BarcodeSettings.Margin,
                    barWidthMultiplier = x.BarcodeSettings.BarWidthMultiplier
                }
                : null
        }).ToArray()
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadBarcodeSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadBarcodeSettingsAsync()
    {
        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, token) => s.GetAsync(token),
            afterSend: response => _barcodeSettings = response,
            createScope: true
        );
    }

    private async Task RemoveItem(CoinItemVm context)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            $"آیا برای حذف سکه {context.CoinInstance.Coin?.Title} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.CoinItems.Remove(context);
            await ReleaseReservedBarcode(context);
            StateHasChanged();
        }
    }

    private async Task AddItem()
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.InvoiceType, InvoiceType.Purchase },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("افزودن سکه جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            await ReserveBarcode(coinItem);

            coinItem.RecalculateAmounts();
            coinItem.Index = GetLastItemIndexNumber() + 1;
            Model.CoinItems.Add(coinItem);

            StateHasChanged();
        }
    }

    public int GetLastItemIndexNumber()
    {
        return Model.CoinItems.Count > 0 ? Model.CoinItems.Max(i => i.Index) : 0;
    }

    private async Task EditItem(CoinItemVm context)
    {
        if (PriceUnit is null)
            return;

        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.Model, context },
            { x => x.PriceUnit, new GetPriceUnitTitleResponse(PriceUnit.Id, PriceUnit.Title, false, false, false) }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm resultItem })
        {
            context.UpdateFrom(resultItem);

            await ReserveBarcode(context);

            StateHasChanged();
        }
    }

    private string GetWeight(CoinItemVm context)
    {
        var weight = context.CoinInstance.Weight?.ToWeightFormat(GoldUnitType.Gram);

        var vacuumedWeight = context.CoinInstance.CoinPackage?.VacuumedWeight?.ToWeightFormat(GoldUnitType.Gram);

        return vacuumedWeight is not null
            ? $"{weight} ({vacuumedWeight} با وکیوم)"
            : weight ?? "-";
    }

    private string GetMintYear(CoinItemVm coinItem)
    {
        var mintYear = coinItem.CoinInstance.MintYear;

        if (!mintYear.HasValue)
            return "نامشخص";

        var persianYear = new PersianCalendar().GetYear(mintYear.Value);

        return persianYear.ToString();
    }

    private async Task PrintBarcode(CoinItemVm item)
    {
        var data = new
        {
            barcode = item.CoinInstance.Barcode,
            productName = item.CoinInstance.Coin?.Title ?? "",
            weight = "",
            wage = ""
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
    }

    private async Task ReserveBarcode(CoinItemVm item)
    {
        if (item.CoinInstance.Id.HasValue)
            return;

        if (!string.IsNullOrWhiteSpace(item.CoinInstance.Barcode))
            return;

        var request = new IssueNextBarcodeRequest(BarcodeType.Coin, null, null, null);

        await SendRequestAsync<IBarcodeReservationService, IssueNextBarcodeResponse>(
            action: (svc, ct) => svc.IssueNextAsync(request, ct),
            afterSend: resp =>
            {
                item.CoinInstance.Barcode = resp.Barcode;
            });
    }

    private async Task ReleaseReservedBarcode(CoinItemVm item)
    {
        if (item.CoinInstance.Id.HasValue)
            return;

        if (string.IsNullOrWhiteSpace(item.CoinInstance.Barcode))
            return;

        await SendRequestAsync<IBarcodeReservationService>(
            action: (svc, ct) => svc.ReleaseAsync(BarcodeType.Coin, item.CoinInstance.Barcode, ct));
    }
}