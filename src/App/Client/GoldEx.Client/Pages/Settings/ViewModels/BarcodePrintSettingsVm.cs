using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class BarcodePrintSettingsVm
{
    [Required(ErrorMessage = "عرض برچسب الزامی است")]
    [Range(100, 1000, ErrorMessage = "عرض برچسب باید بین 100 تا 1000 پیکسل باشد")]
    public int LabelWidth { get; set; } = 300;

    [Required(ErrorMessage = "ارتفاع برچسب الزامی است")]
    [Range(50, 500, ErrorMessage = "ارتفاع برچسب باید بین 50 تا 500 پیکسل باشد")]
    public int LabelHeight { get; set; } = 150;

    [Range(0, 50, ErrorMessage = "حاشیه بالا باید بین 0 تا 50 باشد")]
    public int MarginTop { get; set; } = 5;

    [Range(0, 50, ErrorMessage = "حاشیه راست باید بین 0 تا 50 باشد")]
    public int MarginRight { get; set; } = 5;

    [Range(0, 50, ErrorMessage = "حاشیه پایین باید بین 0 تا 50 باشد")]
    public int MarginBottom { get; set; } = 5;

    [Range(0, 50, ErrorMessage = "حاشیه چپ باید بین 0 تا 50 باشد")]
    public int MarginLeft { get; set; } = 5;

    [Range(0, 50, ErrorMessage = "فاصله داخلی بالا باید بین 0 تا 50 باشد")]
    public int PaddingTop { get; set; } = 10;

    [Range(0, 50, ErrorMessage = "فاصله داخلی راست باید بین 0 تا 50 باشد")]
    public int PaddingRight { get; set; } = 10;

    [Range(0, 50, ErrorMessage = "فاصله داخلی پایین باید بین 0 تا 50 باشد")]
    public int PaddingBottom { get; set; } = 10;

    [Range(0, 50, ErrorMessage = "فاصله داخلی چپ باید بین 0 تا 50 باشد")]
    public int PaddingLeft { get; set; } = 10;

    public List<BarcodePositionItemVm> PositionItems { get; set; } = new();

    public static BarcodePrintSettingsVm CreateFrom(GetBarcodePrintSettingsResponse response)
    {
        return new BarcodePrintSettingsVm
        {
            LabelWidth = response.LabelWidth,
            LabelHeight = response.LabelHeight,
            MarginTop = response.MarginTop,
            MarginRight = response.MarginRight,
            MarginBottom = response.MarginBottom,
            MarginLeft = response.MarginLeft,
            PaddingTop = response.PaddingTop,
            PaddingRight = response.PaddingRight,
            PaddingBottom = response.PaddingBottom,
            PaddingLeft = response.PaddingLeft,
            PositionItems = response.PositionItems
                .Select(BarcodePositionItemVm.CreateFrom)
                .ToList()
        };
    }

    public UpdateBarcodePrintSettingsRequest ToRequest()
    {
        return new UpdateBarcodePrintSettingsRequest(
            LabelWidth,
            LabelHeight,
            MarginTop,
            MarginRight,
            MarginBottom,
            MarginLeft,
            PaddingTop,
            PaddingRight,
            PaddingBottom,
            PaddingLeft,
            PositionItems.Select(x => x.ToRequest()).ToList()
        );
    }

    public List<BarcodePositionItemVm> GetItemsForPosition(BarcodePosition position)
    {
        return PositionItems
            .Where(x => x.Position == position)
            .OrderBy(x => x.Order)
            .ToList();
    }

    public void AddItem(BarcodePositionItemVm item)
    {
        PositionItems.Add(item);
    }

    public void RemoveItem(BarcodePositionItemVm item)
    {
        PositionItems.Remove(item);
    }

    public void ClearPosition(BarcodePosition position)
    {
        PositionItems.RemoveAll(x => x.Position == position);
    }
}