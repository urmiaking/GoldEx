using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class MeltingBatchForm
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public Guid? Id { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    private string CurrentStepTitle => _activeIndex switch { 0 => "انتخاب اجناس", 1 => "ارسال به آزمایشگاه", _ => "ثبت آبشده نهایی" };

    private MeltingBatchVm _model = new();
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetFinancialAccountTitleResponse> _financialAccounts = [];
    private Guid? _id;

    private MudStepper? _stepper;
    private int _activeIndex;
    private bool _processing;

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

    // --- Corrected Event Handlers ---
    private async Task HandleMeltingProducts()
    {
        if (_stepper is null) return;
        var result = await ConfirmTransitionAsync("خروج اجناس از موجودی", "آیا از ثبت درخواست اطمینان دارید؟ با انجام این عملیات، اقلام انتخاب شده از انبار خارج و وارد فرایند ذوب خواهند شد", "ثبت و آغاز ذوب");

        if (result == true)
        {
            _processing = true;
            StateHasChanged(); // Refresh UI to show spinner
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

        var result = await ConfirmTransitionAsync("ارسال به آزمایشگاه", "آیا از ثبت درخواست اطمینان دارید؟ با انجام این عملیات، اقلام ذوب شده برای آزمایشگاه ارسال خواهند شد", "تأیید ارسال");

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
        if (_stepper is null) return;

        var result = await ConfirmTransitionAsync("ثبت آبشده در انبار", "آیا از اتمام فرایند اطمینان دارید؟ با انجام این عملیات، فرایند ذوب به پایان خواهد رسید و اطلاعات مربوط به طلای آبشده در انبار ثبت خواهد شد.", "اتمام فرایند");

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

    private async Task<bool> ConfirmTransitionAsync(string title, string message, string action)
    {
        var dialog = ResponsiveDialogs.Show<MeltingBatchConfirmDialog>(title,
            new Dictionary<string, object>
            {
                [nameof(MeltingBatchConfirmDialog.Message)] = message,
                [nameof(MeltingBatchConfirmDialog.ActionLabel)] = action
            });
        return await dialog.Result is { Canceled: false, Data: true };
    }

    private void OnIndexChanged(int index)
    {
        _activeIndex = index;
        StateHasChanged();
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
        var dialog = ResponsiveDialogs.Show<FinancialAccountEditor>("افزودن حساب مالی جدید",
            new Dictionary<string, object>
            {
                [nameof(FinancialAccountEditor.PriceUnits)] = _priceUnits,
                [nameof(FinancialAccountEditor.IsSystemAccount)] = true,
                [nameof(FinancialAccountEditor.SubmitIndependently)] = true
            }, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadFinancialAccountsAsync();
            StateHasChanged();
        }
    }
}