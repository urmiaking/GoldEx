using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class CategoriesList
{
    private MudTable<ProductCategoryVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<ProductCategoryVm> _productCategories = new List<ProductCategoryVm>();

    private IProductCategoryClientService CategoryService => GetRequiredService<IProductCategoryClientService>();

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var responseList = await CategoryService.GetAllAsync(CancellationTokenSource.Token);

            _productCategories = responseList.Select((item, index) =>
            {
                var vm = ProductCategoryVm.CreateFrom(item);
                vm.Index = index + 1;
                return vm;
            }).ToList();
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private async Task OnCreateCategory()
    {
        var dialog = await DialogService.ShowAsync<Create>("افزودن دسته جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته جدید با موفقیت افزوده شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnEditCategory(ProductCategoryVm model)
    {
        var parameters = new DialogParameters<Update>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Update>("ویرایش دسته", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته مورد نظر با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
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