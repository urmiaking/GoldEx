namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class ReportSummaryVm
{
    public List<SummarySection> Sections { get; set; } = [];
}

public class SummarySection
{
    public string Title { get; set; } = string.Empty;
    public string Info { get; set; } = string.Empty;
    public List<SummaryItem> Items { get; set; } = [];
    public SummaryFooter? Footer { get; set; }
}

public class SummaryItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Type of the summary item, can be "debit", "credit", or "net"
    /// </summary>
    public string Type { get; set; } = string.Empty; // debit, credit, net  

    /// <summary>
    /// Value type of the summary item, can be "debit-value", "credit-value", "positive", or "negative"
    /// </summary>
    public string ValueType { get; set; } = string.Empty; // debit-value, credit-value, positive, negative  
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Icon type of the summary item, can be "debit-icon", "credit-icon", "positive-icon", or "negative-icon"
    /// </summary>
    public string IconType { get; set; } = string.Empty; // debit-icon, credit-icon, positive-icon, negative-icon  
}

public class SummaryFooter
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}