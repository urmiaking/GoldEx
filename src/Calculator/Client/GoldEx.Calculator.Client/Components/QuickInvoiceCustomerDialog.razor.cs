using Blazored.LocalStorage;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class QuickInvoiceCustomerDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private readonly QuickInvoiceCustomerVm _model = new();
    private bool _companyInfoHasSavedValue;

    protected override async Task OnInitializedAsync()
    {
        await LoadCompanyInfoAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCompanyInfoAsync()
    {
        if (!OperatingSystem.IsBrowser())
            return;

        try
        {
            var stored = await LocalStorage.GetItemAsync<QuickInvoiceCompanyInfoDto>(LocalStorageKeys.QuickInvoiceCompanyInfo);
            if (stored is null)
                return;

            if (!string.IsNullOrWhiteSpace(stored.CompanyName))
                _model.CompanyName = stored.CompanyName;

            if (!string.IsNullOrWhiteSpace(stored.CompanyPhone))
                _model.CompanyPhone = stored.CompanyPhone;

            if (!string.IsNullOrWhiteSpace(stored.CompanyAddress))
                _model.CompanyAddress = stored.CompanyAddress;

            if (!string.IsNullOrWhiteSpace(stored.CompanyLogo))
                _model.CompanyLogo = stored.CompanyLogo;

            _companyInfoHasSavedValue =
                !string.IsNullOrWhiteSpace(_model.CompanyName)
                || !string.IsNullOrWhiteSpace(_model.CompanyPhone)
                || !string.IsNullOrWhiteSpace(_model.CompanyAddress)
                || !string.IsNullOrWhiteSpace(_model.CompanyLogo);
        }
        catch
        {
            // ignore: localStorage might not be available (private mode/blocked)
        }
    }

    private async Task PersistCompanyInfoIfChangedAsync()
    {
        if (!OperatingSystem.IsBrowser())
            return;

        try
        {
            var current = new QuickInvoiceCompanyInfoDto
            {
                CompanyName = _model.CompanyName,
                CompanyPhone = _model.CompanyPhone,
                CompanyAddress = _model.CompanyAddress,
                CompanyLogo = _model.CompanyLogo
            };

            var stored = await LocalStorage.GetItemAsync<QuickInvoiceCompanyInfoDto>(LocalStorageKeys.QuickInvoiceCompanyInfo);

            if (stored is not null
                && string.Equals(stored.CompanyName, current.CompanyName, StringComparison.Ordinal)
                && string.Equals(stored.CompanyPhone, current.CompanyPhone, StringComparison.Ordinal)
                && string.Equals(stored.CompanyAddress, current.CompanyAddress, StringComparison.Ordinal)
                && string.Equals(stored.CompanyLogo, current.CompanyLogo, StringComparison.Ordinal))
            {
                return;
            }

            await LocalStorage.SetItemAsync(LocalStorageKeys.QuickInvoiceCompanyInfo, current);
        }
        catch
        {
            // ignore: localStorage might not be available (private mode/blocked)
        }
    }

    private sealed class QuickInvoiceCompanyInfoDto
    {
        public string? CompanyName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyLogo { get; set; }
    }

    private void Cancel() => MudDialog.Cancel();

    private async Task SubmitAsync()
    {
        await _form.ValidateAsync();

        if (_form.IsValid)
        {
            await PersistCompanyInfoIfChangedAsync();
            MudDialog.Close(DialogResult.Ok(_model));
        }
    }

    private async Task UploadLogoAsync(InputFileChangeEventArgs e)
    {
        try
        {
            var file = e.File;
            if (file == null) return;

            if (file.Size > 512 * 1024)
            {
                Snackbar.Add("حجم فایل لوگو نباید بیشتر از ۵۰۰ کیلوبایت باشد", Severity.Error);
                return;
            }

            var buffer = new byte[file.Size];
            using var stream = file.OpenReadStream(512 * 1024);
            await stream.ReadExactlyAsync(buffer);

            var base64 = Convert.ToBase64String(buffer);
            var contentType = file.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "image/png";
            }
            _model.CompanyLogo = $"data:{contentType};base64,{base64}";
            StateHasChanged();
        }
        catch (Exception)
        {
            Snackbar.Add("خطا در بارگذاری لوگو", Severity.Error);
        }
    }

    private void ClearLogo()
    {
        _model.CompanyLogo = null;
        StateHasChanged();
    }
}