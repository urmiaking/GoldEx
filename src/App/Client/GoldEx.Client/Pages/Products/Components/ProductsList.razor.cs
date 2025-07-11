using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Xml.Linq;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Pages.Products.Components;

public partial class ProductsList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private string? _searchString;
    private string _jsVersion = new Random().Next(1, 1000).ToString();
    private MudTable<ProductVm> _table = new ();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small};
    private ProductStatus _productStatus = ProductStatus.Available;
    private DateRange? _filterDateRange;

    private string DateRangeFilterLabel => _productStatus == ProductStatus.Available ? "تاریخ ثبت جنس" : "تاریخ فروش جنس";
    private string ProductStatusIcon => _productStatus == ProductStatus.Available ? Icons.Material.Filled.Warehouse : Icons.Material.Filled.ShoppingBasket;

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task<TableData<ProductVm>> LoadProductsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<ProductVm>();

        var productFilter = new ProductFilter(_productStatus, _filterDateRange?.Start, _filterDateRange?.End);

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IProductService, PagedList<GetProductResponse>>(
            action: (service, token) => service.GetListAsync(filter, productFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(ProductVm.CreateFrom).ToList();
                result = new TableData<ProductVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            }
        );

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await RefreshAsync();
    }

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }

    public async Task OnCreateProduct()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن جنس جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس جدید با موفقیت افزوده شد.");
            await RefreshAsync();
        }
    }

    private async Task OnRemoveProduct(ProductVm model)
    {
        if (!model.Id.HasValue)
            return;

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id.Value },
            { x => x.ProductName, model.Name }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت حذف شد.");
            await RefreshAsync();
        }
    }

    private async Task OnEditProduct(ProductVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت ویرایش شد.");
            await RefreshAsync();
        }
    }

    private async Task OnPrintBarcode(ProductVm product)
    {
        var labelData = new
        {
            text = product.Barcode,
            name = product.Name,
            weight = "وزن: " + product.Weight?.ToString("G29") + "g",
            wage = "اجرت: " + product.WageType switch
            {
                WageType.Fixed => $"{product.Wage?.ToCurrencyFormat(product.WagePriceUnitTitle)}",
                WageType.Percent => product.Wage?.ToString("G29") + "%",
                _ => "ندارد"
            }
        };

        await JsRuntime.InvokeVoidAsync("printBarcode", labelData);
    }

    private async Task SetStatusFilterText(ProductStatus filterType)
    {
        _productStatus = filterType;
        await RefreshAsync();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private void OnViewInvoice(Guid? invoiceId)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = invoiceId }));
    }
}