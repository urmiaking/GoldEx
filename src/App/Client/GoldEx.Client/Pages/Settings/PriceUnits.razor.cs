using GoldEx.Client.Pages.Settings.Components.PriceUnits;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class PriceUnits
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<PriceUnitVm> _priceUnits = new List<PriceUnitVm>();

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitResponse>>(
            action: (s, ct) => s.GetAllAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response.Select((item, index) =>
                {
                    var vm = PriceUnitVm.CreateFrom(item);
                    vm.Index = index + 1;
                    return vm;
                });
            });
    }

    private async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن واحد ارزی جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("واحد ارزی جدید با موفقیت افزوده شد.");
            await LoadPriceUnitsAsync();
        }
    }

    private async Task OnEdit(PriceUnitVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model },
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش واحد ارزی", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("واحد ارزی مورد نظر با موفقیت ویرایش شد.");
            await LoadPriceUnitsAsync();
        }
    }

    private async Task OnStatusChanged(PriceUnitVm model)
    {
        var request = new UpdatePriceUnitStatusRequest(!model.IsActive);

        await SendRequestAsync<IPriceUnitService>((s, ct) => s.UpdateStatusAsync(model.Id, request, ct));

        AddSuccessToast("وضعیت واحد ارزی با موفقیت تغییر کرد.");
        await LoadPriceUnitsAsync();
    }

    private async Task OnSetAsDefault(Guid id)
    {
        await SendRequestAsync<IPriceUnitService>((s, ct) => s.SetAsDefaultAsync(id, ct), afterSend: async () =>
        {
            AddSuccessToast("واحد ارزی پیش‌فرض با موفقیت تغییر کرد.");
            await LoadPriceUnitsAsync();
        });
    }
}