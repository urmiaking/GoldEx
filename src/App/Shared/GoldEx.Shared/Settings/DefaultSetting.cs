namespace GoldEx.Shared.Settings;

public class DefaultSetting
{
    public string InstitutionName { get; set; } = default!;
    public string Address { get; set; } = "تهران، خیابان ولیعصر، میدان ولیعصر، پلاک 1";
    public string PhoneNumber { get; set; } = "09905492104";
    public decimal TaxPercent { get; set; }
    public decimal GoldProfitPercent { get; set; }
    public decimal JewelryProfitPercent { get; set; }
    public TimeSpan PriceUpdateInterval { get; set; } = TimeSpan.FromMinutes(1);
}