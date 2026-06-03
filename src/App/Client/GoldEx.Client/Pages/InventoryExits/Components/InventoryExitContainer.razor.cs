using FluentValidation;
using GoldEx.Client.Pages.InventoryExits.Validators;
using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class InventoryExitContainer
{
    [Parameter] public string? Class { get; set; }

    private readonly InventoryExitVm _model = new() { ExitDate = DateTime.Now };
    private GetPriceUnitResponse? _priceUnit;
    private MudForm _form = default!;
    private readonly InventoryExitValidator _inventoryExitValidator = new ();

    protected override async Task OnInitializedAsync()
    {
        await LoadDefaultPriceUnitAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDefaultPriceUnitAsync()
    {
        await SendRequestAsync<IPriceUnitService, GetPriceUnitResponse?>(
            action: (s, ct) => s.GetDefaultAsync(ct),
            afterSend: response => _priceUnit = response);
    }

    private async Task OnSubmitAsync()
    {
        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        try
        {
            _model.ToRequest();
        }
        catch (ValidationException e)
        {
            AddErrorToast(e.Message);
            return;
        }

        var request = _model.ToRequest();

        await SendRequestAsync<IInventoryExitService>(
            action: (s, ct) => s.ExitAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast("عملیات با موفقیت انجام شد");
                Navigation.NavigateTo(ClientRoutes.InventoryStocks.InventoryExits.List);
                return Task.CompletedTask;
            });
    }
}