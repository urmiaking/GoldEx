using GoldEx.Client.Pages.Reporting.VIewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components;

public partial class ReportGenerator
{
    [Parameter, EditorRequired] public ReportType ReportType { get; set; }

    private ReportGeneratorVm _model = new();
    private List<GetCustomerResponse> _customers = [];

    private Task OnSubmit()
    {
        return Task.CompletedTask;
    }

    private async Task<IEnumerable<GetCustomerResponse>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, ct),
            afterSend: response => _customers = response,
            cancelPrevious: true);

        return _customers;
    }
}