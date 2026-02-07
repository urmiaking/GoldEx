namespace GoldEx.Client.Pages.Register.ViewModels;

public class ProductUpgradeVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public string DiscountText { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = [];
    public bool IsSelected { get; set; }

    public decimal DiscountPercentage => OriginalPrice / Price;
}