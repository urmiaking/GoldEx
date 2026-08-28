using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.ProductAttributes;

public partial class ProductAttributeEditorDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public ProductAttributeDto? Model { get; set; }

    private MudForm _form = default!;
    private bool IsEdit => Model != null;
    private bool _isSaving;

    private string _title = string.Empty;
    private string? _unit;
    private ProductAttributeDataType _dataType = ProductAttributeDataType.Text;
    private string? _options;
    private string? _description;

    protected override void OnInitialized()
    {
        if (Model != null)
        {
            _title = Model.Title;
            _unit = Model.Unit;
            _dataType = Model.DataType;
            _options = Model.Options;
            _description = Model.Description;
        }
    }

    private async Task SaveAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSaving = true;
        try
        {
            if (IsEdit && Model != null)
            {
                var request = new UpdateProductAttributeRequest(
                    _title,
                    _unit,
                    _dataType,
                    _options,
                    _description);

                await SendRequestAsync<IProductAttributeService>(
                    action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                    afterSend: () =>
                    {
                        AddSuccessToast("ویژگی با موفقیت ویرایش شد.");
                        MudDialog.Close(DialogResult.Ok(true));
                        return Task.CompletedTask;
                    });
            }
            else
            {
                var request = new CreateProductAttributeRequest(
                    _title,
                    _unit,
                    _dataType,
                    _options,
                    _description);

                await SendRequestAsync<IProductAttributeService>(
                    action: (s, ct) => s.CreateAsync(request, ct),
                    afterSend: () =>
                    {
                        AddSuccessToast("ویژگی جدید با موفقیت ایجاد شد.");
                        MudDialog.Close(DialogResult.Ok(true));
                        return Task.CompletedTask;
                    });
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}
