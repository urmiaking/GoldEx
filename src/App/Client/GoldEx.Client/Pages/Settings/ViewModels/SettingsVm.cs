using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Settings;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class SettingsVm
{
    public Guid Id { get; set; }

    [Display(Name = "نام گالری")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string InstitutionName { get; set; } = default!;

    [Display(Name = "آدرس")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Address { get; set; } = default!;

    [Display(Name = "شماره تلفن")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string PhoneNumber { get; set; } = default!;

    [Display(Name = "مالیات")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal TaxPercent { get; set; }

    [Display(Name = "سود طلا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal GoldProfitPercent { get; set; }
    
    [Display(Name = "سود جواهر")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal JewelryProfitPercent { get; set; }

    [Display(Name = "کارمزد طلای آب شده")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal MoltenGoldCommissionPercent { get; set; }

    [Display(Name = "حاشیه اطمینان قیمت طلا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal GoldSafetyMarginPercent { get; set; }

    [Display(Name = "نرخ کاهش عیار طلای کهنه")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public decimal UsedGoldFinenessDeductionRate { get; set; }

    [Display(Name = "هر گرم مثقال")]
    public decimal GramPerMesghal { get; set; }

    [Display(Name = "زمان بروز رسانی قیمت ها")]
    public TimeSpan PriceUpdateInterval { get; set; }

    [Display(Name = "لوگوی گالری")]
    public IBrowserFile? IconFile { get; set; }

    [Display(Name = "لوگوی گالری")]
    public byte[]? IconContent { get; set; }

    public bool HasIcon { get; set; }

    [Display(Name = "لینک اینستاگرام")]
    public string? InstagramUrl { get; set; }

    [Display(Name = "کانال / آیدی تلگرام")]
    public string? TelegramUrl { get; set; }

    [Display(Name = "کانال / آیدی بله")]
    public string? BaleUrl { get; set; }

    [Display(Name = "شماره واتساپ استعلام")]
    public string? WhatsAppNumber { get; set; }

    [Display(Name = "درباره گالری (متن معرفی ویترین)")]
    public string? AboutText { get; set; }

    [Display(Name = "پریست و تم ویترین")]
    public string VitrineThemePreset { get; set; } = "royal-emerald";

    [Display(Name = "رنگ اصلی برند")]
    public string? VitrinePrimaryColor { get; set; }

    [Display(Name = "رنگ طلایی و اکسنت")]
    public string? VitrineAccentColor { get; set; }

    [Display(Name = "رنگ پس‌زمینه سایت")]
    public string? VitrineBackgroundColor { get; set; }

    [Display(Name = "رنگ پس‌زمینه کارت‌ها")]
    public string? VitrineSurfaceColor { get; set; }

    [Display(Name = "استایل کارت‌ها")]
    public string VitrineCardStyle { get; set; } = "minimal";

    [Display(Name = "انحنای گوشه‌ها")]
    public string VitrineRadiusStyle { get; set; } = "rounded";

    [Display(Name = "فونت ویترین")]
    public string VitrineFontStyle { get; set; } = "iransans";

    [Display(Name = "استایل هدر")]
    public string VitrineHeaderStyle { get; set; } = "glass-sticky";

    public UpdateSettingRequest ToRequest()
    {
        return new UpdateSettingRequest(InstitutionName,
            Address,
            PhoneNumber,
            TaxPercent,
            GoldProfitPercent,
            JewelryProfitPercent,
            MoltenGoldCommissionPercent,
            PriceUpdateInterval,
            GoldSafetyMarginPercent,
            UsedGoldFinenessDeductionRate,
            GramPerMesghal,
            IconContent,
            InstagramUrl,
            TelegramUrl,
            BaleUrl,
            WhatsAppNumber,
            AboutText,
            VitrineThemePreset,
            VitrinePrimaryColor,
            VitrineAccentColor,
            VitrineBackgroundColor,
            VitrineSurfaceColor,
            VitrineCardStyle,
            VitrineRadiusStyle,
            VitrineFontStyle,
            VitrineHeaderStyle);
    }

    public static SettingsVm CreateFromRequest(GetSettingResponse response)
    {
        return new SettingsVm
        {
            Id = response.Id,
            InstitutionName = response.InstitutionName,
            Address = response.Address,
            PhoneNumber = response.PhoneNumber,
            TaxPercent = response.TaxPercent,
            GoldProfitPercent = response.GoldProfitPercent,
            JewelryProfitPercent = response.JewelryProfitPercent,
            MoltenGoldCommissionPercent = response.MoltenGoldCommissionPercent,
            PriceUpdateInterval = response.PriceUpdateInterval,
            GoldSafetyMarginPercent = response.GoldSafetyMarginPercent,
            UsedGoldFinenessDeductionRate = response.UsedGoldFinenessDeductionRate,
            GramPerMesghal = response.GramPerMesghal,
            HasIcon = response.HasIcon,
            InstagramUrl = response.InstagramUrl,
            TelegramUrl = response.TelegramUrl,
            BaleUrl = response.BaleUrl,
            WhatsAppNumber = response.WhatsAppNumber,
            AboutText = response.AboutText,
            VitrineThemePreset = string.IsNullOrWhiteSpace(response.VitrineThemePreset) ? "royal-emerald" : response.VitrineThemePreset,
            VitrinePrimaryColor = response.VitrinePrimaryColor,
            VitrineAccentColor = response.VitrineAccentColor,
            VitrineBackgroundColor = response.VitrineBackgroundColor,
            VitrineSurfaceColor = response.VitrineSurfaceColor,
            VitrineCardStyle = string.IsNullOrWhiteSpace(response.VitrineCardStyle) ? "minimal" : response.VitrineCardStyle,
            VitrineRadiusStyle = string.IsNullOrWhiteSpace(response.VitrineRadiusStyle) ? "rounded" : response.VitrineRadiusStyle,
            VitrineFontStyle = string.IsNullOrWhiteSpace(response.VitrineFontStyle) ? "iransans" : response.VitrineFontStyle,
            VitrineHeaderStyle = string.IsNullOrWhiteSpace(response.VitrineHeaderStyle) ? "glass-sticky" : response.VitrineHeaderStyle
        };
    }
}