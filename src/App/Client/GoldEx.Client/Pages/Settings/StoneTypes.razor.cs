using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.Components.StoneTypes;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings;

public partial class StoneTypes
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<StoneTypeVm> _stoneTypes = new List<StoneTypeVm>();
    private bool _processing;

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "stone-types-management";
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadStoneTypesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadStoneTypesAsync()
    {
        _processing = true;

        await SendRequestAsync<IStoneTypeService, List<GetStoneTypeResponse>>(
            action: (s, ct) => s.GetListAsync(new StoneTypeRequestFilter(null, null), ct),
            afterSend: response =>
            {
                _stoneTypes = response.Select((item, index) =>
                {
                    var vm = StoneTypeVm.CreateFrom(item);
                    vm.Index = index + 1;
                    return vm;
                });

                _processing = false;
            });
    }

    private async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن سنگ جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سنگ جدید با موفقیت افزوده شد.");
            await LoadStoneTypesAsync();
        }
    }

    private async Task OnEdit(StoneTypeVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model },
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش سنگ", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سنگ مورد نظر با موفقیت ویرایش شد.");
            await LoadStoneTypesAsync();
        }
    }

    private async Task OnToggleStatus(StoneTypeVm model)
    {
        await SendRequestAsync<IStoneTypeService>((s, ct) => s.ToggleStatusAsync(model.Id, ct));

        AddSuccessToast("وضعیت سنگ با موفقیت تغییر کرد.");
        await LoadStoneTypesAsync();
    }
}
