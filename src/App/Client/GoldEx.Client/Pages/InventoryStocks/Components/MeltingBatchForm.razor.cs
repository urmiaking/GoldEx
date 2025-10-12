using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class MeltingBatchForm
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public Guid? Id { get; set; }

    private MeltingBatchVm _model = new();
    private GetSettingResponse? _setting;
    private List<GetCustomerNameResponse> _assayers = [];
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private Guid? _id;

    private MudAutocomplete<GetCustomerNameResponse> _assayerField = new();

    private MudStepper? _stepper;
    private int _activeIndex = 0;
    private bool _processing;
    private bool _feeFieldMenuOpen;
    private List<GetFinancialAccountTitleResponse> _financialAccounts = [];

    private string? FeeExchangeRateLabel =>
        _model is { FeePriceUnit: not null, PriceUnit: not null }
            ? $"نرخ تبدیل {_model.FeePriceUnit.Title} به {_model.PriceUnit.Title}"
            : null;

    protected override async Task OnParametersSetAsync()
    {
        if (_id != Id)
        {
            _id = Id;
            await LoadMeltingBatchAsync();
        }

        await LoadPriceUnitsAsync();
        await LoadGramPriceAsync();
        await LoadFinancialAccountsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, _model.FeePriceUnit?.Id, ct),
            afterSend: response => _financialAccounts = response);
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;
                _model.PriceUnit = response.FirstOrDefault(x => x.IsDefault);
                _model.FeePriceUnit = response.FirstOrDefault(x => x.IsDefault);
                StateHasChanged();
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(_model.WeightUnitType, _model.PriceUnit?.Id, false, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPrice);
                _model.GramPrice = gramPrice;
                StateHasChanged();
            });
    }

    private async Task LoadMeltingBatchAsync()
    {
        if (Id.HasValue)
        {
            await SendRequestAsync<IMeltingBatchService, GetMeltingBatchResponse>(
                action: (s, ct) => s.GetAsync(Id.Value, ct),
                afterSend: response =>
                {
                    _model = MeltingBatchVm.CreateFrom(response);

                    _activeIndex = response.CurrentStatus switch
                    {
                        MeltingBatchStatus.Melting => 1,
                        MeltingBatchStatus.SentToLab => 2,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                });
        }
    }

    private async Task HandleMeltingProducts()
    {
        if (_stepper is null)
            return;

        var result = await DialogService.ShowMessageBox(
            "تأیید عملیات",
            "آیا از ثبت درخواست اطمینان دارید؟ با انجام این عملیات، اقلام انتخاب شده از انبار خارج و وارد فرایند ذوب خواهند شد",
            yesText: "بله",
            noText: "خیر");

        if (result == true)
        {
            _processing = true;
            StateHasChanged();

            await SendRequestAsync<IMeltingBatchService, CreateMeltingBatchResponse>(
                action: (s, ct) => s.CreateAsync(_model.ToMeltingRequest(), ct),
                afterSend: async response =>
                {
                    AddSuccessToast("اقلام انتخابی با موفقیت برای ذوب ثبت شدند");
                    _id = response.Id;
                    _processing = false;
                    await _stepper.NextStepAsync();
                },
                onFailure: () =>
                {
                    _processing = false;
                    return Task.CompletedTask;
                });
        }
    }

    private async Task HandleSendToLab()
    {
        if (_stepper is null)
            return;

        if (_model.Assayer is null)
        {
            AddErrorToast("لطفا آزمایشگاه را مشخص کنید");
            await _assayerField.FocusAsync();
            return;
        }

        var result = await DialogService.ShowMessageBox(
            "تأیید عملیات",
            "آیا از ثبت درخواست اطمینان دارید؟ با انجام این عملیات، اقلام ذوب شده برای آزمایشگاه ارسال خواهند شد",
            yesText: "بله",
            noText: "خیر");

        if (result == true && _id.HasValue)
        {
            _processing = true;
            StateHasChanged();

            await SendRequestAsync<IMeltingBatchService>(
                action: (s, ct) => s.SendToLabAsync(_id.Value, _model.ToSendToLabRequest(), ct),
                afterSend: async () =>
                {
                    AddSuccessToast("درخواست با موفقیت ثبت شد");
                    _processing = false;
                    await _stepper.NextStepAsync();
                },
                onFailure: () =>
                {
                    _processing = false;
                    return Task.CompletedTask;
                });
        }
    }

    private async Task HandleCompleteProcess()
    {
        if (_stepper is null)
            return;

        var result = await DialogService.ShowMessageBox(
            "تأیید عملیات",
            "آیا از اتمام فرایند اطمینان دارید؟ با انجام این عملیات، فرایند ذوب به پایان خواهد رسید و اطلاعات مربوط به طلای آبشده در انبار ثبت خواهد شد.",
            yesText: "بله",
            noText: "خیر");

        if (result == true && _id.HasValue)
        {
            _processing = true;
            StateHasChanged();

            await SendRequestAsync<IMeltingBatchService>(
                action: (s, ct) => s.CompleteMeltingAsync(_id.Value, _model.ToCompleteMeltingRequest(), ct),
                afterSend: () =>
                {
                    AddSuccessToast("فرایند با موفقیت به پایان رسید");
                    _processing = false;
                    Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Index);
                    return Task.CompletedTask;
                },
                onFailure: () =>
                {
                    _processing = false;
                    return Task.CompletedTask;
                });
        }
    }

    private void OnIndexChanged(int index)
    {
        _activeIndex = index;
        StateHasChanged();
    }

    private void OnSelectedItemsChanged(HashSet<InventoryStockVm>? inventoryItems)
    {
        _model.Products = inventoryItems?.Where(x => x.Product != null)
            .Select(x => x.Product!).ToList() ?? [];

        (_model.WeightUnitType, _model.TotalWeight) = _model.GetWeightParams(_setting?.GramPerMesghal);
        StateHasChanged();
    }

    private async Task<IEnumerable<GetCustomerNameResponse>?> SearchAssayers(string? assayerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assayerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerNameResponse>>(
            action: (s, ct) => s.GetNamesAsync(assayerName, CustomerType.AssayingLab, ct),
            afterSend: response => _assayers = response,
            cancelPrevious: true);

        return _assayers;
    }

    private async Task OnAddAssayer()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.Model, new CustomerVm(){ CustomerType = CustomerType.AssayingLab }}
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن طرف حساب جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            _model.Assayer = new GetCustomerNameResponse(customerVm.Id!.Value, customerVm.FullName);
            StateHasChanged();
        }
    }

    private void BackToList()
    {
        Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Index);
    }

    private async Task SelectFeePriceUnit(GetPriceUnitTitleResponse item)
    {
        _model.FeePriceUnit = item;

        if (_model is { FeePriceUnit: not null, PriceUnit: not null })
        {
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(_model.FeePriceUnit.Id, _model.PriceUnit.Id, ct),
                afterSend: async response =>
                {
                    _model.FeeExchangeRate = response.ExchangeRate;
                    await LoadFinancialAccountsAsync();
                    StateHasChanged();
                });
        }
    }

    private async Task OnAddFinancialAccount()
    {
        DialogOptions dialogOptions = new()
        {
            CloseButton = true,
            FullWidth = true,
            FullScreen = false,
            MaxWidth = MaxWidth.Small
        };

        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, _priceUnits },
            { x => x.IsSystemAccount, true },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadFinancialAccountsAsync();
            StateHasChanged();
        }
    }

    private int GetDescriptionMd()
    {
        var used = 3; // FeeAmount always visible

        if (_model.FeePriceUnit != _model.PriceUnit)
            used += 3; // Exchange rate field

        if (_model.FeeAmount.HasValue)
            used += 3; // Financial account field

        return 12 - used; // Remaining for description
    }
}