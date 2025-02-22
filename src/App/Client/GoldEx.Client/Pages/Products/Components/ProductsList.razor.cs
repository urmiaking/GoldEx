using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class ProductsList
{
    private MudTable<ProductVm>? _table;
    private IProductClientService ProductService => GetRequiredService<IProductClientService>();

    private Task OnRemoveProduct(ProductVm model)
    {
        throw new NotImplementedException();
    }

    private Task OnEditProduct(ProductVm model)
    {
        throw new NotImplementedException();
    }

    private async Task<TableData<ProductVm>> LoadProductsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<ProductVm>();
        using var scope = CreateServiceScope();
        var service = GetRequiredService<IProductClientService>(scope);

        try
        {
            SetBusy();
            CancelToken();

            var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
                state.SortDirection switch
                {
                    SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                    SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                    SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                    _ => throw new ArgumentOutOfRangeException()
                });

            var response = await service.GetListAsync(filter, cancellationToken);

            var items = response.Data.Select(ProductVm.CreateFrom).ToList();

            result = new TableData<ProductVm>
            {
                TotalItems = response.Total,
                Items = items
            };
        }
        catch (Exception ex)
        {
            AddExceptionToast(ex);
        }
        finally
        {
            SetIdeal();
        }

        return result;
    }
}