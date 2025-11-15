using GoldEx.Client.Pages.Invoices.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class ExcelUploadDialog
{
    private IReadOnlyList<IBrowserFile>? _files;
    private IBrowserFile? _selectedFile;
    private string? _fileName;
    private bool _fileReady;

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

    private void DownloadTemplate()
    {
        // If you have a static template file in wwwroot/templates
        var url = "/templates/product-import-template.xlsx";
        JsRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private async Task HandleUpload()
    {
        if (_selectedFile is null)
            return;

        await using var stream = _selectedFile.OpenReadStream(long.MaxValue);
        var items = await ParseExcelAsync(stream);

        MudDialog.Close(DialogResult.Ok(items));
    }

    private void Cancel() => MudDialog.Cancel();

    private Task<List<ProductItemVm>> ParseExcelAsync(Stream stream)
    {
        // TODO: Implement EPPlus or NPOI parser
        return Task.FromResult(new List<ProductItemVm>());
    }
}