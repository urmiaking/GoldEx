using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class ReportDesigner
{
    [Parameter] public string Name { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? DisplayName { get; set; }
}