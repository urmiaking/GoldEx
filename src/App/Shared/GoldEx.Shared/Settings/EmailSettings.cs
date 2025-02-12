namespace GoldEx.Shared.Settings;

public class EmailSettings
{
    public string SmtpServer { get; set; } = default!;
    public int SmtpPort { get; set; } = default!;
    public string SmtpUsername { get; set; } = default!;
    public string SmtpPassword { get; set; } = default!;
    public string SenderName { get; set; } = default!;
    public string SenderEmail { get; set; } = default!;
}