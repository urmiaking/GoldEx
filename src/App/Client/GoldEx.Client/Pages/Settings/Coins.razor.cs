using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.Components.Coins;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace GoldEx.Client.Pages.Settings;

public partial class Coins
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private List<CoinVm> _coins = [];
    private bool _processing;

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "coin-management";
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCoinsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCoinsAsync()
    {
        _processing = true;
        await SendRequestAsync<ICoinService, List<GetCoinResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response =>
            {
                _coins = response.Select(CoinVm.CreateFrom).ToList();
                _processing = false;
            });
    }

    private async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن سکه جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سکه جدید با موفقیت افزوده شد.");
            await LoadCoinsAsync();
        }
    }

    private async Task OnEdit(CoinVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سکه مورد نظر با موفقیت ویرایش شد.");
            await LoadCoinsAsync();
        }
    }

    private async Task OnStatusChanged(CoinVm context)
    {
        if (!context.Id.HasValue)
            return;

        await SendRequestAsync<ICoinService>(
            action: (s, ct) => s.SetStatusAsync(context.Id.Value, !context.IsActive, ct),
            afterSend: async () =>
            {
                AddSuccessToast("وضعیت سکه با موفقیت تغییر کرد");
                await LoadCoinsAsync();
            });
    }

    private async Task OnDelete(CoinVm context)
    {
        var coinId = context.Id ?? throw new InvalidOperationException();

        var result = await DialogService.ShowMessageBoxAsync(
            "حذف سکه",
            $"آیا از حذف سکه '{context.Title}' اطمینان دارید؟ این عملیات قابل بازگشت نیست.",
            yesText: "حذف کن",
            noText: "انصراف",
            options: new DialogOptions { FullWidth = true, MaxWidth = MaxWidth.ExtraSmall });

        if (result == true)
        {
            await SendRequestAsync<ICoinService>(
                action: (s, ct) => s.DeleteAsync(coinId, ct),
                afterSend: async () =>
                {
                    AddSuccessToast("سکه مورد نظر با موفقیت حذف شد.");
                    await LoadCoinsAsync();
                });
        }
    }

    private static string GetMintYearText(DateTime? start, DateTime? end)
    {
        if ((start is null || start.Value.Year == 0) && end is null)
            return "نامشخص";

        var pc = new PersianCalendar();

        var startYear = start is not null && start.Value.Year > 0
            ? pc.GetYear(start.Value).ToString()
            : "نامشخص";

        var endYear = end is null
            ? "تاکنون"
            : pc.GetYear(end.Value).ToString();

        return $"{startYear} - {endYear}";
    }
}