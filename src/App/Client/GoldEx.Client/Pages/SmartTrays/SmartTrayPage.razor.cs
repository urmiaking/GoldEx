using GoldEx.Client.Components.Components;
using GoldEx.Client.Components.Components.SmartTrays;
using GoldEx.Shared.DTOs.SmartTrays;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.SmartTrays;

public enum ScanMode
{
    OutToShowroom = 1,
    ReturnToShowcase = 2
}

public partial class SmartTrayPage
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public bool IsEmbedded { get; set; }
    [Parameter] public int Elevation { get; set; } = 2;

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = null!;

    private List<SmartTrayDto> _activeTrays = [];
    private SmartTrayDto? _currentTray;
    private Guid _selectedTrayId;
    private ScanMode _scanMode = ScanMode.OutToShowroom;

    private string _barcodeInput = string.Empty;
    private MudTextField<string>? _barcodeInputRef;
    private bool _isProcessingBarcode;
    private bool _showCameraScanner;
    private bool _isLoading = true;

    private readonly HashSet<string> _selectedItemBarcodes = [];
    private string _elapsedTimeString = "00:00:00";
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _timerCts;

    private DotNetObjectReference<SmartTrayPage>? _dotNetRef;
    private HubConnection? _hubConnection;

    private int _auditProgressPercentage =>
        _currentTray is null || _currentTray.TotalItemCount == 0
            ? 0
            : (int)Math.Round((double)(_currentTray.ReturnedCount + _currentTray.SoldCount) / _currentTray.TotalItemCount * 100);

    protected override async Task OnInitializedAsync()
    {
        if (!IsEmbedded && !Id.HasValue)
        {
            Navigation.NavigateTo($"{ClientRoutes.Calculator.Index}?tab=smart-tray", replace: true);
            return;
        }

        await LoadActiveTraysAsync();

        if (Id.HasValue && Id.Value != Guid.Empty)
        {
            _selectedTrayId = Id.Value;
            await LoadSelectedTrayAsync(_selectedTrayId, showLoading: true);
        }
        else if (_activeTrays.Any())
        {
            _selectedTrayId = _activeTrays.First().Id;
            await LoadSelectedTrayAsync(_selectedTrayId, showLoading: true);
        }
        else
        {
            _isLoading = false;
        }

        StartElapsedTimer();
        await StartSignalRAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                await JsRuntime.InvokeVoidAsync("GoldExSmartTrayScanner.initialize", _dotNetRef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SmartTray] Hardware scanner init error: {ex.Message}");
            }
        }
    }

    private void StartElapsedTimer()
    {
        _timerCts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = Task.Run(async () =>
        {
            while (_timer != null && await _timer.WaitForNextTickAsync(_timerCts.Token))
            {
                if (_currentTray is not null)
                {
                    var elapsed = DateTime.UtcNow - _currentTray.CreatedAt;
                    _elapsedTimeString = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    await InvokeAsync(StateHasChanged);
                }
            }
        });
    }

    private async Task StartSignalRAsync()
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetService<HttpClient>();
            var baseAddress = httpClient?.BaseAddress?.ToString().TrimEnd('/');

            var hubUrl = !string.IsNullOrEmpty(baseAddress)
                ? $"{baseAddress}{ApiRoutes.Hubs.SmartTrays}"
                : ApiRoutes.Hubs.SmartTrays;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<SmartTrayDto>("TrayUpdated", (tray) =>
            {
                InvokeAsync(() =>
                {
                    var index = _activeTrays.FindIndex(x => x.Id == tray.Id);
                    if (index >= 0) _activeTrays[index] = tray;
                    else if (tray.Status == SmartTrayStatus.Active || tray.Status == SmartTrayStatus.Auditing)
                        _activeTrays.Insert(0, tray);

                    if (_selectedTrayId == tray.Id)
                    {
                        _currentTray = tray;
                    }
                    StateHasChanged();
                });
            });

            _hubConnection.On<Guid, SmartTrayItemDto>("ItemAdded", (trayId, item) =>
            {
                if (_selectedTrayId == trayId)
                {
                    InvokeAsync(() =>
                    {
                        if (_currentTray != null && !_currentTray.Items.Any(x => x.Barcode.Equals(item.Barcode, StringComparison.OrdinalIgnoreCase)))
                        {
                            _currentTray.Items.Add(item);
                            StateHasChanged();
                        }
                    });
                }
            });

            _hubConnection.On<Guid, SmartTrayItemDto>("ItemReturned", (trayId, item) =>
            {
                if (_selectedTrayId == trayId)
                {
                    InvokeAsync(() =>
                    {
                        if (_currentTray != null)
                        {
                            var existing = _currentTray.Items.FirstOrDefault(x => x.Barcode.Equals(item.Barcode, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                var idx = _currentTray.Items.IndexOf(existing);
                                _currentTray.Items[idx] = item;
                                StateHasChanged();
                            }
                        }
                    });
                }
            });

            _hubConnection.On<Guid, SmartTrayItemDto>("ItemSold", (trayId, item) =>
            {
                if (_selectedTrayId == trayId)
                {
                    InvokeAsync(() =>
                    {
                        if (_currentTray != null)
                        {
                            var existing = _currentTray.Items.FirstOrDefault(x => x.Barcode.Equals(item.Barcode, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                var idx = _currentTray.Items.IndexOf(existing);
                                _currentTray.Items[idx] = item;
                                StateHasChanged();
                            }
                        }
                    });
                }
            });

            _hubConnection.On<Guid, string>("ItemRemoved", (trayId, barcode) =>
            {
                if (_selectedTrayId == trayId)
                {
                    InvokeAsync(() =>
                    {
                        if (_currentTray != null)
                        {
                            _currentTray.Items.RemoveAll(x => x.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase));
                            _selectedItemBarcodes.Remove(barcode);
                            StateHasChanged();
                        }
                    });
                }
            });

            _hubConnection.On<Guid, string>("DiscrepancyReported", (trayId, _) =>
            {
                InvokeAsync(async () =>
                {
                    await LoadActiveTraysAsync();
                    if (_selectedTrayId == trayId)
                        await LoadSelectedTrayAsync(trayId);
                });
            });

            _hubConnection.On<Guid>("TrayClosed", (trayId) =>
            {
                InvokeAsync(async () =>
                {
                    await LoadActiveTraysAsync();
                    if (_selectedTrayId == trayId)
                    {
                        _currentTray = null;
                        if (_activeTrays.Any())
                        {
                            _selectedTrayId = _activeTrays.First().Id;
                            await LoadSelectedTrayAsync(_selectedTrayId);
                        }
                    }
                });
            });

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartTray] SignalR hub connection failed: {ex.Message}");
        }
    }

    private async Task LoadActiveTraysAsync()
    {
        await SendRequestAsync<ISmartTrayService, List<SmartTrayDto>>(
            action: (s, ct) => s.GetActiveTraysAsync(ct),
            afterSend: list =>
            {
                _activeTrays = list;
            });
    }

    private async Task LoadSelectedTrayAsync(Guid trayId, bool showLoading = false)
    {
        if (showLoading)
        {
            _isLoading = true;
            StateHasChanged();
        }

        await SendRequestAsync<ISmartTrayService, SmartTrayDto>(
            action: (s, ct) => s.GetTrayByIdAsync(trayId, ct),
            afterSend: tray =>
            {
                _currentTray = tray;
                _selectedTrayId = tray.Id;
                _selectedItemBarcodes.Clear();
                _isLoading = false;
            },
            onFailure: () =>
            {
                _isLoading = false;
            });
    }

    private async Task OnTraySelectionChanged(Guid newTrayId)
    {
        if (newTrayId == Guid.Empty || newTrayId == _selectedTrayId) return;
        _selectedTrayId = newTrayId;
        await LoadSelectedTrayAsync(newTrayId, showLoading: true);
    }

    private void SetScanMode(ScanMode mode)
    {
        _scanMode = mode;
        StateHasChanged();
    }

    private void ToggleCameraScanner()
    {
        _showCameraScanner = !_showCameraScanner;
    }

    private async Task OnBarcodeKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await OnSubmitManualBarcodeAsync();
        }
    }

    private async Task OnSubmitManualBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(_barcodeInput) || _isProcessingBarcode) return;
        var code = _barcodeInput.Trim();
        _barcodeInput = string.Empty;
        await OnBarcodeProcessedAsync(code);
    }

    [JSInvokable]
    public async Task OnHardwareBarcodeScanned(string barcode)
    {
        await InvokeAsync(() => OnBarcodeProcessedAsync(barcode));
    }

    private async Task OnBarcodeProcessedAsync(string rawBarcode)
    {
        if (string.IsNullOrWhiteSpace(rawBarcode) || _currentTray is null || _isProcessingBarcode) return;

        var barcode = rawBarcode.Trim();
        _isProcessingBarcode = true;
        StateHasChanged();

        try
        {
            if (_scanMode == ScanMode.OutToShowroom)
            {
                await SendRequestAsync<ISmartTrayService, SmartTrayItemDto>(
                    action: (s, ct) => s.AddItemByBarcodeAsync(_selectedTrayId, barcode, ct),
                    afterSend: async item =>
                    {
                        AddSuccessToast($"کالای «{item.Title}» به سینی افزوده شد.");
                        await PlayAudioAsync("playScanSuccess");
                        await LoadSelectedTrayAsync(_selectedTrayId);
                    },
                    onFailure: async () =>
                    {
                        await PlayAudioAsync("playWarning");
                    });
            }
            else
            {
                await SendRequestAsync<ISmartTrayService, SmartTrayItemDto>(
                    action: (s, ct) => s.ReturnItemByBarcodeAsync(_selectedTrayId, barcode, ct),
                    afterSend: async item =>
                    {
                        AddSuccessToast($"کالای «{item.Title}» به ویترین بازگردانده شد.");
                        await PlayAudioAsync("playReturnSuccess");
                        await LoadSelectedTrayAsync(_selectedTrayId);
                    },
                    onFailure: async () =>
                    {
                        await PlayAudioAsync("playWarning");
                    });
            }
        }
        finally
        {
            _isProcessingBarcode = false;
            StateHasChanged();
        }
    }

    private async Task MarkItemReturnedAsync(string barcode)
    {
        await SendRequestAsync<ISmartTrayService, SmartTrayItemDto>(
            action: (s, ct) => s.ReturnItemByBarcodeAsync(_selectedTrayId, barcode, ct),
            afterSend: async item =>
            {
                AddSuccessToast($"کالای «{item.Title}» به ویترین بازگردانده شد.");
                await PlayAudioAsync("playReturnSuccess");
                await LoadSelectedTrayAsync(_selectedTrayId);
            });
    }

    private async Task ReAddReturnedItemAsync(string barcode)
    {
        await SendRequestAsync<ISmartTrayService, SmartTrayItemDto>(
            action: (s, ct) => s.AddItemByBarcodeAsync(_selectedTrayId, barcode, ct),
            afterSend: async item =>
            {
                AddSuccessToast($"کالای «{item.Title}» مجدداً روی پیش‌خوان قرار گرفت.");
                await PlayAudioAsync("playScanSuccess");
                await LoadSelectedTrayAsync(_selectedTrayId);
            });
    }

    private async Task RemoveItemFromTrayAsync(string barcode)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "تایید حذف",
            $"آیا از حذف کالا با بارکد {barcode} از سینی اطمینان دارید؟",
            yesText: "بله، حذف کن",
            cancelText: "لغو");

        if (result != true) return;

        await SendRequestAsync<ISmartTrayService>(
            action: (s, ct) => s.RemoveItemAsync(_selectedTrayId, barcode, ct),
            afterSend: async () =>
            {
                AddSuccessToast("کالا از سینی حذف شد.");
                await LoadSelectedTrayAsync(_selectedTrayId);
            });
    }

    private async Task OpenCreateTrayDialogAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateSmartTrayDialog>("ایجاد سینی جدید");
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CreateSmartTrayRequest req })
        {
            await SendRequestAsync<ISmartTrayService, SmartTrayDto>(
                action: (s, ct) => s.CreateTrayAsync(req, ct),
                afterSend: async newTray =>
                {
                    AddSuccessToast($"سینی #{newTray.TrayNumber} با موفقیت ایجاد شد.");
                    await LoadActiveTraysAsync();
                    await LoadSelectedTrayAsync(newTray.Id);
                });
        }
    }

    private void ToggleItemSelection(string barcode, bool isSelected)
    {
        if (isSelected) _selectedItemBarcodes.Add(barcode);
        else _selectedItemBarcodes.Remove(barcode);
    }

    private bool IsAllSelected() =>
        _currentTray != null &&
        _currentTray.Items.Any(x => x.Status != SmartTrayItemStatus.Sold) &&
        _currentTray.Items.Where(x => x.Status != SmartTrayItemStatus.Sold).All(x => _selectedItemBarcodes.Contains(x.Barcode));

    private void ToggleSelectAll(bool selectAll)
    {
        if (_currentTray is null) return;
        _selectedItemBarcodes.Clear();
        if (selectAll)
        {
            foreach (var item in _currentTray.Items.Where(x => x.Status != SmartTrayItemStatus.Sold))
            {
                _selectedItemBarcodes.Add(item.Barcode);
            }
        }
    }

    private void TransferSelectedToInvoice()
    {
        if (!_selectedItemBarcodes.Any()) return;
        var joinedBarcodes = string.Join(",", _selectedItemBarcodes);
        Navigation.NavigateTo($"/invoices/create?invoiceType=Sell&barcodes={Uri.EscapeDataString(joinedBarcodes)}");
    }

    private async Task CloseOrAuditTrayAsync()
    {
        if (_currentTray is null) return;

        var unresolved = _currentTray.Items.Where(x => x.Status == SmartTrayItemStatus.InTray).ToList();

        if (unresolved.Count == 0)
        {
            // All items returned or sold -> Perfect closing
            var confirm = await DialogService.ShowMessageBoxAsync(
                "تکمیل و بستن سینی",
                "تمام اقلام سینی به ویترین بازگردانده شده یا فروخته شده‌اند. آیا از بستن این سینی اطمینان دارید؟",
                yesText: "بله، سینی بسته شود",
                cancelText: "انصراف");

            if (confirm != true) return;

            await SendRequestAsync<ISmartTrayService>(
                action: (s, ct) => s.CloseTrayAsync(_selectedTrayId, ct),
                afterSend: async () =>
                {
                    AddSuccessToast("سینی با موفقیت تکمیل و بسته شد.");
                    await PlayAudioAsync("playTrayCompleted");
                    await LoadActiveTraysAsync();
                    _currentTray = null;
                    if (_activeTrays.Any())
                        await LoadSelectedTrayAsync(_activeTrays.First().Id);
                });
        }
        else
        {
            // Unresolved items remain -> Alarm & Discrepancy Dialog
            await PlayAudioAsync("playDiscrepancyAlarm");

            var parameters = new DialogParameters<SmartTrayDiscrepancyDialog>
            {
                { x => x.UnresolvedItems, unresolved }
            };

            var dialog = await DialogService.ShowAsync<SmartTrayDiscrepancyDialog>("ثبت مغایرت", parameters, new DialogOptions { MaxWidth = MaxWidth.Small });
            var result = await dialog.Result;

            if (result is { Canceled: false, Data: string notes })
            {
                var req = new ReportSmartTrayDiscrepancyRequest { Notes = notes };
                await SendRequestAsync<ISmartTrayService>(
                    action: (s, ct) => s.ReportDiscrepancyAsync(_selectedTrayId, req, ct),
                    afterSend: async () =>
                    {
                        AddWarningToast("مغایرت سینی ثبت گردید و سینی بسته شد.");
                        await LoadActiveTraysAsync();
                        _currentTray = null;
                        if (_activeTrays.Any())
                            await LoadSelectedTrayAsync(_activeTrays.First().Id);
                    });
            }
        }
    }

    private async Task CancelTrayAsync()
    {
        var confirm = await DialogService.ShowMessageBoxAsync(
            "هشدار لغو سینی",
            "آیا از لغو این سینی اطمینان دارید؟ تمام اقلام از وضعیت سینی خارج خواهند شد.",
            yesText: "بله، لغو شود",
            cancelText: "انصراف");

        if (confirm != true) return;

        await SendRequestAsync<ISmartTrayService>(
            action: (s, ct) => s.CancelTrayAsync(_selectedTrayId, ct),
            afterSend: async () =>
            {
                AddWarningToast("سینی با موفقیت لغو شد.");
                await LoadActiveTraysAsync();
                _currentTray = null;
                if (_activeTrays.Any())
                    await LoadSelectedTrayAsync(_activeTrays.First().Id);
            });
    }

    private async Task PlayAudioAsync(string methodName)
    {
        try
        {
            await JsRuntime.InvokeVoidAsync($"GoldExSmartTrayAudio.{methodName}");
        }
        catch { }
    }

    private static Color GetTrayStatusColor(SmartTrayStatus status) => status switch
    {
        SmartTrayStatus.Active => Color.Warning,
        SmartTrayStatus.Auditing => Color.Info,
        SmartTrayStatus.Completed => Color.Success,
        SmartTrayStatus.HasDiscrepancy => Color.Error,
        _ => Color.Default
    };

    private static Color GetItemStatusColor(SmartTrayItemStatus status) => status switch
    {
        SmartTrayItemStatus.InTray => Color.Warning,
        SmartTrayItemStatus.Returned => Color.Success,
        SmartTrayItemStatus.Sold => Color.Info,
        SmartTrayItemStatus.Missing => Color.Error,
        _ => Color.Default
    };

    private static string GetItemStatusIcon(SmartTrayItemStatus status) => status switch
    {
        SmartTrayItemStatus.InTray => Icons.Material.Filled.PanTool,
        SmartTrayItemStatus.Returned => Icons.Material.Filled.CheckCircle,
        SmartTrayItemStatus.Sold => Icons.Material.Filled.Receipt,
        SmartTrayItemStatus.Missing => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.Info
    };

    private static string FormatItemDuration(SmartTrayItemDto item)
    {
        TimeSpan span;
        if (item.Status == SmartTrayItemStatus.InTray)
        {
            span = DateTime.UtcNow - item.AddedAt;
        }
        else if (item.ReturnedAt.HasValue)
        {
            span = item.ReturnedAt.Value - item.AddedAt;
        }
        else if (item.SoldAt.HasValue)
        {
            span = item.SoldAt.Value - item.AddedAt;
        }
        else
        {
            span = TimeSpan.FromSeconds(item.DurationSeconds);
        }

        if (span < TimeSpan.Zero) span = TimeSpan.Zero;

        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";

        return $"{span.Minutes:D2}:{span.Seconds:D2}";
    }

    public override async ValueTask DisposeAsync()
    {
        _timerCts?.Cancel();
        _timer?.Dispose();

        try
        {
            await JsRuntime.InvokeVoidAsync("GoldExSmartTrayScanner.dispose");
        }
        catch { }

        _dotNetRef?.Dispose();

        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
