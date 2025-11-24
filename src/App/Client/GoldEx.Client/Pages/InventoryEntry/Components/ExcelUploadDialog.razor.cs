using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
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
    private bool _processing;
    private double _progressValue;
    private string _progressMessage = string.Empty;
    private ProcessExcelResponse? _response;
    private CancellationTokenSource? _simulationCts;

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
        await _selectedFile.OpenReadStream(maxAllowedSize: 15 * 1024 * 1024).CopyToAsync(ms);
        var bytes = ms.ToArray();

        await ProcessExcelAsync(bytes);
    }

    private void Close() => MudDialog.Close();

    private async Task ProcessExcelAsync(byte[] bytes)
    {
        var request = new ProcessExcelRequest(bytes);

        // Start progress simulation
        _simulationCts = new CancellationTokenSource();
        var simulationTask = SimulateProgressAsync(_simulationCts.Token);

        try
        {
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
        finally
        {
            // Cancel simulation and cleanup
            await _simulationCts.CancelAsync();
            try { await simulationTask; } catch (OperationCanceledException) { }
            _simulationCts.Dispose();
            _simulationCts = null;
        }
    }

    private void OnBack()
    {
        _response = null;
        _fileReady = false;
        _fileName = null;
        _selectedFile = null;
        _files = null;
        _progressValue = 0;
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

    private async Task SimulateProgressAsync(CancellationToken ct)
    {
        // Phase 1: 0% to 50% in 10 seconds
        _progressValue = 0;
        _progressMessage = "در حال آماده سازی اطلاعات...";
        StateHasChanged();

        // 50 steps over 10,000ms = 200ms per step
        for (var i = 0; i <= 50; i++)
        {
            if (ct.IsCancellationRequested) return;
            _progressValue = i;
            StateHasChanged();
            await Task.Delay(200, ct);
        }

        // Pause 1 second before transition
        await Task.Delay(1000, ct);

        // Phase 2: 51% to 99% in 10 seconds
        _progressMessage = "درحال بهینه سازی اجناس با هوش مصنوعی...";
        StateHasChanged();

        // 49 steps over 10,000ms ~= 205ms per step
        for (var i = 51; i <= 99; i++)
        {
            if (ct.IsCancellationRequested) return;
            _progressValue = i;
            StateHasChanged();
            await Task.Delay(205, ct);
        }

        // Pause 1 second before final message
        await Task.Delay(1000, ct);

        // Phase 3: Final waiting state
        _progressMessage = "در حال آماده سازی نهایی...";
        StateHasChanged();

        await Task.Delay(Timeout.Infinite, ct);
    }
}