using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.PriceUnits;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public PriceUnitVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private IEnumerable<PriceVm> _prices = [];

    protected override async Task OnParametersSetAsync()
    {
        await LoadPricesAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceService, List<GetPriceTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync([MarketType.Currency, MarketType.Gold], ct),
            afterSend: response => _prices = response.Select(PriceVm.CreateFrom));
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        byte[]? uploadedFile = null;

        if (Model.IconFile is not null)
        {
            await using var stream = Model.IconFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            uploadedFile = memoryStream.ToArray();
        }

        if (Id is null)
        {
            var request = PriceUnitVm.ToCreateRequest(Model, uploadedFile);
            await SendRequestAsync<IPriceUnitService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = PriceUnitVm.ToUpdateRequest(Model, uploadedFile);
            await SendRequestAsync<IPriceUnitService>(
                action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void Close() => MudDialog.Cancel();

    private void OnPriceVmChanged(PriceVm? price)
    {
        if (price is null)
            return;

        Model.PriceVm = price;
        Model.PriceId = price.Id;
    }
}