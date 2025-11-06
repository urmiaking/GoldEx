using GoldEx.Shared.DTOs.Prices;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class PriceSettingVm
{
    public IBrowserFile? IconFile { get; set; }

    public static UpdatePriceSettingRequest ToRequest(byte[] uploadedFile)
    {
        return new UpdatePriceSettingRequest(uploadedFile);
    }
}