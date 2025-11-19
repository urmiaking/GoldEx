using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class ExcelUploadDialog
{
    private IReadOnlyList<IBrowserFile>? _files;
    private IBrowserFile? _selectedFile;
    private string? _fileName;
    private bool _fileReady;
    private bool _processing;
    private ProcessExcelResponse? _response;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private void HandleFilesChanged()
    {
        if (_files is not { Count: > 0 })
        {
            _selectedFile = null;
            _fileReady = false;
            return;
        }

        _selectedFile = _files[0];
        _fileName = _selectedFile.Name;
        _fileReady = true;

        StateHasChanged();
    }

    private async Task DownloadTemplate()
    {
        var url = ApiUrls.Files.GetInventoryEntryTemplate();
        await JsRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private async Task HandleUpload()
    {
        if (_selectedFile is null)
            return;

        _processing = true;

        await using var ms = new MemoryStream();
        await _selectedFile.OpenReadStream().CopyToAsync(ms);
        var bytes = ms.ToArray();

        await ProcessExcelAsync(bytes);
    }

    private void Close() => MudDialog.Close();

    private async Task ProcessExcelAsync(byte[] bytes)
    {
        var request = new ProcessExcelRequest(bytes);

        await SendRequestAsync<IInventoryEntryService, ProcessExcelResponse>(
            action: (s, ct) => s.ProcessExcelAsync(request, ct),
            afterSend: response =>
            {
                _response = response;
                _processing = false;
            },
            onFailure: () => _processing = false
        );
    }

    private void OnBack()
    {
        _response = null;
        _fileReady = false;
        _fileName = null;
        _selectedFile = null;
        _files = null;
    }

    private void OnConfirmImport()
    {
        if (_response is null)
            return;

        _processing = true;

        var result = _response.Items.Select(ProductItemVm.CreateFromItem).ToList();

        _processing = false;

        MudDialog.Close(DialogResult.Ok(result));
    }
}