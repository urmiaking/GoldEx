namespace GoldEx.Shared.Settings;

public class DefaultSetting
{
    public string InstitutionName { get; set; } = default!;
    public string Address { get; set; } = "تهران، خیابان ولیعصر، میدان ولیعصر، پلاک 1";
    public string PhoneNumber { get; set; } = "09905492104";
    public float TaxPercent { get; set; }
    public float GoldProfitPercent { get; set; }
    public float JewelryProfitPercent { get; set; }
}