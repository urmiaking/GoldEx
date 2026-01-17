using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.SmsTemplates;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings;

public partial class SmsTemplates
{
    private List<SmsTemplateResponse>? _items;

    private SmsTemplateVm _model = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadSmsTemplatesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSmsTemplatesAsync()
    {
        await SendRequestAsync<ISmsTemplateService, List<SmsTemplateResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _items = response;
                _model = SmsTemplateVm.CreateFrom(response);
            });
    }

    private async Task OnSubmit(EditContext arg)
    {
        await SendRequestAsync<ISmsTemplateService>(
            action: (s, ct) => s.UpdateAsync(_model.ToRequests(), ct),
            afterSend: () =>
            {
                AddSuccessToast("قالب پیامکی با موفقیت ذخیره شد");
                return Task.CompletedTask;
            });
    }
}