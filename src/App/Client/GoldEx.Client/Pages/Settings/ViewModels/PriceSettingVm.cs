using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class PriceSettingVm
{
    public IBrowserFile? IconFile { get; set; }
    public PriceProviderType ProviderType { get; set; } = PriceProviderType.Manual;
    public string? ProviderSymbol { get; set; }

    public static UpdatePriceSettingRequest ToRequest(byte[] icon, PriceProviderType providerType, string? providerSymbol, bool enabled)
        => new(icon, providerType, providerSymbol, enabled);

    public static PriceSettingVm CreateFrom(PriceSettingDto price)
    {
        return new PriceSettingVm
        {
            ProviderType = price.ProviderType ?? PriceProviderType.Manual,
            ProviderSymbol = price.ProviderSymbol
        };
    }
}