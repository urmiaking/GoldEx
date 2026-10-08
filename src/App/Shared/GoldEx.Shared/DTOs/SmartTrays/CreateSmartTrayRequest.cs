namespace GoldEx.Shared.DTOs.SmartTrays;

public class CreateSmartTrayRequest
{
    public int? TrayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ClerkName { get; set; }
}
