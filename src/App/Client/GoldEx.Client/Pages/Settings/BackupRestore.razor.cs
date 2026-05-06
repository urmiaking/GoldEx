using GoldEx.Shared.DTOs.Backups;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace GoldEx.Client.Pages.Settings;

public partial class BackupRestore
{
    private IBrowserFile? _databaseFile;
    private bool _processing;

    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private async Task DownloadBackup()
    {
        await JsRuntime.InvokeVoidAsync("eval", $"window.location.href = '{ApiUrls.Backups.GetFilePath()}'");
    }

    private async Task RestoreFile()
    {
        if (_databaseFile is null)
            return;

        var result = await DialogService.ShowMessageBoxAsync("آیا مطمئن هستید؟",
            $"آیا برای بازنشانی دیتابیس با فایل '{_databaseFile.Name}' اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            _processing = true;
            StateHasChanged();
            var stream = _databaseFile.OpenReadStream(
                maxAllowedSize: 2L * 1024 * 1024 * 1024);

            var request = new RestoreDatabaseRequest(
                stream,
                _databaseFile.Name);

            await SendRequestAsync<IBackupService>((s, ct) => s.RestoreDatabaseAsync(request, ct),
                afterSend: () =>
                {
                    AddSuccessToast("بازنشانی با موفقیت انجام شد");
                    _databaseFile = null;
                    _processing = false;
                    return Task.CompletedTask;
                },
                onFailure: () =>
                {
                    _processing = false;
                    return Task.CompletedTask;
                });

            StateHasChanged();
        }
    }
}