using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class CustomerComprehensiveReportPrint
{
    [Parameter, SupplyParameterFromQuery(Name = "customerId")] public Guid CustomerId { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "includeDetails")] public bool IncludeDetails { get; set; } = true;
    [Parameter, SupplyParameterFromQuery(Name = "landscape")] public bool LandscapeMode { get; set; } = true;

    private CustomerComprehensiveRpResponse? _report;
    private GetCustomerResponse? _customer;

    protected override async Task OnInitializedAsync()
    {
        if (CustomerId != Guid.Empty)
        {
            var request = new CustomerComprehensiveRpRequest(
                CustomerId, FromDate?.Date, ToDate?.Date.AddDays(1).AddTicks(-1), IncludeDetails);
            _report = await SendRequestAsync<IReportingService, CustomerComprehensiveRpResponse>(
                (service, ct) => service.GetCustomerComprehensiveAsync(request, ct),
                createScope: true);
            _customer = await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                (service, ct) => service.GetAsync(CustomerId, ct),
                createScope: true);
        }
        await base.OnInitializedAsync();
    }
}
