using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components;

public partial class ReportPanel
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;
}