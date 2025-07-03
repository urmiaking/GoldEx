using GoldEx.Client.Pages.Settings.Components.PaymentMethods;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class PaymentMethods
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<PaymentMethodVm> _paymentMethods = new List<PaymentMethodVm>();
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        await LoadPaymentMethodsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPaymentMethodsAsync()
    {
        _processing = true;

        await SendRequestAsync<IPaymentMethodService, List<GetPaymentMethodResponse>>(
            action: (s, ct) => s.GetAllAsync(ct),
            afterSend: response =>
            {
                _paymentMethods = response.Select((item, index) =>
                {
                    var vm = PaymentMethodVm.CreateFrom(item);
                    vm.Index = index + 1;
                    return vm;
                });

                _processing = false;
            });
    }

    private async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن روش پرداخت جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("روش پرداخت جدید با موفقیت افزوده شد.");
            await LoadPaymentMethodsAsync();
        }
    }

    private async Task OnEdit(PaymentMethodVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model },
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش روش پرداخت", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("روش پرداخت مورد نظر با موفقیت ویرایش شد.");
            await LoadPaymentMethodsAsync();
        }
    }

    private async Task OnStatusChanged(PaymentMethodVm model)
    {
        var request = new UpdatePaymentMethodStatusRequest(!model.IsActive);

        await SendRequestAsync<IPaymentMethodService>((s, ct) => s.UpdateStatusAsync(model.Id, request, ct));

        AddSuccessToast("وضعیت روش پرداخت با موفقیت تغییر کرد.");
        await LoadPaymentMethodsAsync();
    }
}