using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CoinItemEditor
{
    [Parameter] public CoinItemVm Model { get; set; } = new() { Quantity = 1 };
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;

    private List<GetCoinResponse> _coins = [];
    private List<GetCustomerResponse> _customers = [];
    private MudForm _form = default!;
    private bool _isProcessing;
    private readonly CoinItemValidator _coinItemValidator = new();
    private int? _maxAvailableAmount;
    private MudColorPicker? _colorPicker;

    protected override async Task OnParametersSetAsync()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Model.Coin != null)
            await LoadMaxAmountAsync(Model.Coin);

        await LoadCoinAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadCoinAsync()
    {
        await SendRequestAsync<ICoinService, List<GetCoinResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response =>
            {
                _coins = response;
                StateHasChanged();
            });
    }

    private async Task LoadMaxAmountAsync(GetCoinResponse coin)
    {
        await SendRequestAsync<IInventoryStockService, GetInventoryStockAmountResponse>(
            action: (s, ct) => s.GetAvailableItemAmountAsync(coin.Id, ItemType.Coin, ct),
            afterSend: response => _maxAvailableAmount = (int)response.Amount);
    }

    public void Close() => MudDialog.Close();

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.Validate();

        if (!_form.IsValid)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = false;
        MudDialog.Close(DialogResult.Ok(Model));
    }

    private async Task OnCoinChanged(GetCoinResponse? coin)
    {
        if (coin is null)
            return;

        Model.Coin = coin;
        Model.Weight = coin.Weight;
        Model.Fineness = coin.Fineness;

        if (Model.MintYear.HasValue)
        {
            if (Model.MintYear.Value.Year < coin.StartMintYear || (coin.EndMintYear.HasValue && Model.MintYear.Value.Year > coin.EndMintYear.Value))
                Model.MintYear = null;
        }

        if (PriceUnit is null)
            return;

        await LoadMaxAmountAsync(coin);

        await SendRequestAsync<ICoinService, GetExchangeRateResponse?>(
            action: (s, ct) => s.GetPriceAsync(coin.Id, PriceUnit.IsDefault ? null : PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                Model.UnitPrice = response.ExchangeRate ?? 0;

                StateHasChanged();
            });
    }

    private int GetMaxAmount()
    {
        if (InvoiceType is InvoiceType.Purchase)
            return int.MaxValue;

        return _maxAvailableAmount ?? 0;
    }

    private void OnCoinMintTypeChanged(CoinMintType coinMintType)
    {
        Model.CoinMintType = coinMintType;

        if (Model.Coin is not null && coinMintType is CoinMintType.Banking)
        {
            Model.Weight = Model.Coin.Weight;
            Model.Fineness = Model.Coin.Fineness;
        }
    }

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, CustomerType.Workshop, ct),
            afterSend: response =>
            {
                _customers = response;
            },
            cancelPrevious: true);

        return _customers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer()
    {
        if (Model.CoinPackage is null)
            return;

        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.CustomerType, CustomerType.Workshop }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن کارگاه جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.CoinPackage.Issuer = customerVm;
            StateHasChanged();
        }
    }

    #endregion

    private async Task OpenColorPicker()
    {
        if (_colorPicker is not null)
            await _colorPicker.OpenAsync();
    }

    private DateTime? GetMinDate()
    {
        if (Model.Coin is null || Model.Coin.StartMintYear is 0)
            return null;

        return new DateTime(Model.Coin.StartMintYear, 1, 1);
    }

    private DateTime? GetMaxDate()
    {
        if (Model.Coin is null || Model.Coin.EndMintYear is null or 0)
            return DateTime.Now;

        return new DateTime(Model.Coin.EndMintYear.Value, 12, 31);
    }

    private void OnCoinPackageTypeChanged(CoinPackageType packageType)
    {
        Model.CoinPackageType = packageType;

        if (packageType is CoinPackageType.VacuumSealed && Model.CoinPackage is null)
            Model.CoinPackage = new CoinPackageSpecVm();
        else 
            Model.CoinPackage = null;
    }
}