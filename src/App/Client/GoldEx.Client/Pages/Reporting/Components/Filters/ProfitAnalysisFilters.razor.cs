using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class ProfitAnalysisFilters
{
    [Parameter] public ProfitAnalysisFilterVm Model { get; set; } = default!;
    private List<GetProductCategoryResponse> _categories = [];
    private GetCustomerResponse? _customer;

    protected override async Task OnInitializedAsync()
    {
        _categories = await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            (s, ct) => s.GetListAsync(ct), createScope: true) ?? [];
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Model.CustomerId.HasValue && _customer?.Id != Model.CustomerId)
            _customer = await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                (s, ct) => s.GetAsync(Model.CustomerId.Value, ct), createScope: true);
        if (!Model.CustomerId.HasValue) _customer = null;
    }

    private void CustomerChanged(GetCustomerResponse? customer)
    {
        _customer = customer;
        Model.CustomerId = customer?.Id;
    }

    private async Task<IEnumerable<GetCustomerResponse>> SearchCustomers(string text, CancellationToken cancellationToken)
        => string.IsNullOrWhiteSpace(text) ? [] :
            await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
                (s, ct) => s.GetByNameAsync(text, null, ct), createScope: true) ?? [];
}
