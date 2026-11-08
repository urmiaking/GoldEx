using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Sdk.Common.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace GoldEx.Client.Pages.Reporting.Pages.Reports;

public partial class CustomerComprehensive
{
    [Parameter, SupplyParameterFromQuery(Name = "customerId")] public Guid? CustomerId { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private readonly CustomerComprehensiveFilterVm _filters = new();
    private CustomerComprehensiveRpResponse? _report;
    private GetCustomerResponse? _customer;
    private bool _loading;
    private bool _failed;
    private bool _printLandscape = true;
    private Guid? _loadedCustomer;

    protected override async Task OnParametersSetAsync()
    {
        if (CustomerId.HasValue && CustomerId != Guid.Empty && CustomerId != _loadedCustomer)
        {
            _filters.CustomerId = CustomerId;
            _loadedCustomer = CustomerId;
            await LoadAsync();
        }
        await base.OnParametersSetAsync();
    }

    private Task OnSubmit(ReportFilterVmBase _) => LoadAsync();

    private async Task LoadAsync()
    {
        if (!_filters.CustomerId.HasValue) return;
        _loadedCustomer = _filters.CustomerId;
        _loading = true;
        _failed = false;
        _report = null;
        await InvokeAsync(StateHasChanged);
        try
        {
            var request = _filters.ToRequest();
            await SendRequestAsync<IReportingService, CustomerComprehensiveRpResponse>(
                (service, ct) => service.GetCustomerComprehensiveAsync(request, ct),
                afterSend: value => _report = value,
                onFailure: () => _failed = true,
                cancelPrevious: true,
                createScope: true);
            await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                (service, ct) => service.GetAsync(request.CustomerId, ct),
                afterSend: value => _customer = value,
                cancelPrevious: true,
                createScope: true);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task PrintReport()
    {
        if (_report == null || !_filters.CustomerId.HasValue) return;
        var url = ClientRoutes.Reporting.Print.CustomerComprehensive.AppendQueryString(new
        {
            customerId = _filters.CustomerId,
            start = _filters.DateRange?.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            end = _filters.DateRange?.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            includeDetails = _filters.IncludeDetails,
            landscape = _printLandscape
        });
        await JS.InvokeVoidAsync("openReportPopup", Navigation.ToAbsoluteUri(url).ToString());
    }
}
