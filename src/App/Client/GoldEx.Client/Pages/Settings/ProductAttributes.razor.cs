using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.Components.ProductAttributes;
using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class ProductAttributes
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Medium };
    private List<ProductAttributeDto> _attributes = [];
    private bool _processing;

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "product-attributes";
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAttributesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadAttributesAsync()
    {
        _processing = true;

        await SendRequestAsync<IProductAttributeService, List<ProductAttributeDto>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _attributes = response ?? [];
                _processing = false;
            });
    }

    private async Task OnCreateAttribute()
    {
        var dialog = await DialogService.ShowAsync<ProductAttributeEditorDialog>("تعریف ویژگی جدید", _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await LoadAttributesAsync();
        }
    }

    private async Task OnEditAttribute(ProductAttributeDto item)
    {
        var parameters = new DialogParameters<ProductAttributeEditorDialog>
        {
            { x => x.Model, item }
        };

        var dialog = await DialogService.ShowAsync<ProductAttributeEditorDialog>("ویرایش ویژگی", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await LoadAttributesAsync();
        }
    }

    private async Task OnDeleteAttribute(ProductAttributeDto item)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            "حذف ویژگی",
            $"آیا از حذف ویژگی «{item.Title}» اطمینان دارید؟",
            yesText: "بله، حذف شود",
            noText: "انصراف");

        if (confirmed == true)
        {
            await SendRequestAsync<IProductAttributeService>(
                action: (s, ct) => s.DeleteAsync(item.Id, ct),
                afterSend: () =>
                {
                    AddSuccessToast($"ویژگی «{item.Title}» با موفقیت حذف شد.");
                    return LoadAttributesAsync();
                });
        }
    }
}
