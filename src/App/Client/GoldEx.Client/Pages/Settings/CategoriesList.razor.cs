using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class CategoriesList
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<ProductCategoryVm> _productCategories = new List<ProductCategoryVm>();
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        _processing = true;

        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _productCategories = response.Select((item, index) =>
                {
                    var vm = ProductCategoryVm.CreateFrom(item);
                    vm.Index = index + 1;
                    return vm;
                });

                _processing = false;
            });
    }

    private async Task OnCreateCategory()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن دسته جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته جدید با موفقیت افزوده شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnEditCategory(ProductCategoryVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model },
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش دسته", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته مورد نظر با موفقیت ویرایش شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnRemoveCategory(ProductCategoryVm model)
    {
        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id },
            { x => x.CategoryName, model.Title }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف دسته", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast($"دسته {model.Title} با موفقیت حذف شد.");
            await LoadCategoriesAsync();
        }
    }
}