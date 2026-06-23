using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class BarcodePrintSettingsVm
{
    [Required(ErrorMessage = "عرض برچسب الزامی است")]
    [Range(30.0, 120.0, ErrorMessage = "عرض برچسب باید بین 30 تا 120 میلی‌متر باشد")]
    public double LabelWidth { get; set; } = 80.0;

    [Required(ErrorMessage = "ارتفاع برچسب الزامی است")]
    [Range(5.0, 80.0, ErrorMessage = "ارتفاع برچسب باید بین 5 تا 80 میلی‌متر باشد")]
    public double LabelHeight { get; set; } = 15.0;

    [Required(ErrorMessage = "عرض دم برچسب الزامی است")]
    [Range(0.0, 50.0, ErrorMessage = "عرض دم برچسب باید بین 0 تا 50 میلی‌متر باشد")]
    public double TailWidth { get; set; } = 30.0;

    [Range(0.0, 20.0, ErrorMessage = "حاشیه بالا باید بین 0 تا 20 میلی‌متر باشد")]
    public double MarginTop { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "حاشیه راست باید بین 0 تا 20 میلی‌متر باشد")]
    public double MarginRight { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "حاشیه پایین باید بین 0 تا 20 میلی‌متر باشد")]
    public double MarginBottom { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "حاشیه چپ باید بین 0 تا 20 میلی‌متر باشد")]
    public double MarginLeft { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "فاصله داخلی بالا باید بین 0 تا 20 میلی‌متر باشد")]
    public double PaddingTop { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "فاصله داخلی راست باید بین 0 تا 20 میلی‌متر باشد")]
    public double PaddingRight { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "فاصله داخلی پایین باید بین 0 تا 20 میلی‌متر باشد")]
    public double PaddingBottom { get; set; } = 1.0;

    [Range(0.0, 20.0, ErrorMessage = "فاصله داخلی چپ باید بین 0 تا 20 میلی‌متر باشد")]
    public double PaddingLeft { get; set; } = 1.0;

    public List<BarcodePositionItemVm> PositionItems { get; set; } = new();

    public static BarcodePrintSettingsVm CreateFrom(GetBarcodePrintSettingsResponse response)
    {
        return new BarcodePrintSettingsVm
        {
            LabelWidth = response.LabelWidth,
            LabelHeight = response.LabelHeight,
            TailWidth = response.TailWidth,
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
            TailWidth,
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