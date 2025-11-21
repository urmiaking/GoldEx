using GoldEx.Client.Pages.Blogs.ViewModels;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Blogs.Components;

public partial class BlogCategories
{
    private List<BlogCategoryResponse> _categories = [];
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        FullScreen = false,
        MaxWidth = MaxWidth.Small
    };

    [Parameter] public EventCallback<BlogPostTitleResponse> OnPostView { get; set; }
    [Parameter] public EventCallback<BlogPostTitleResponse> OnPostEdit { get; set; }
    [Parameter] public EventCallback<BlogCategoryResponse> OnPostCreate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();
        await base.OnInitializedAsync();
    }

    public async Task LoadCategoriesAsync()
    {
        await SendRequestAsync<IBlogCategoryService, List<BlogCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response => _categories = response);
    }

    private async Task OnAddCategory()
    {
        var dialog = await DialogService.ShowAsync<BlogCategoryEditor>("افزودن دسته‌بندی جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته‌بندی جدید با موفقیت افزوده شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnEditCategory(BlogCategoryResponse category)
    {
        var parameters = new DialogParameters<BlogCategoryEditor>
        {
            { x => x.Model, new BlogCategoryVm { Id = category.Id, Title = category.Title } }
        };

        var dialog = await DialogService.ShowAsync<BlogCategoryEditor>("ویرایش دسته‌بندی", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("دسته‌بندی با موفقیت ویرایش شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnDeleteCategory(BlogCategoryResponse category)
    {
        var result = await DialogService.ShowMessageBox("حذف دسته بندی",
            $"آیا برای حذف دسته بندی {category.Title} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IBlogCategoryService>(
                action: (s, ct) => s.DeleteAsync(category.Id, ct),
                afterSend: async () =>
                {
                    AddSuccessToast("دسته‌بندی با موفقیت حذف شد.");
                    await LoadCategoriesAsync();
                });
        }
    }

    private async Task OnCreateSubCategory(BlogCategoryResponse category)
    {
        var parameters = new DialogParameters<BlogCategoryEditor>
        {
            { x => x.Model, new BlogCategoryVm { ParentCategoryId = category.Id } }
        };

        var dialog = await DialogService.ShowAsync<BlogCategoryEditor>("افزودن زیردسته", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("زیردسته با موفقیت افزوده شد.");
            await LoadCategoriesAsync();
        }
    }

    private async Task OnSetCategoryStatus(BlogCategoryResponse category)
    {
        await SendRequestAsync<IBlogCategoryService>(
            action: (s, ct) => s.SetStatusAsync(category.Id, !category.IsActive, ct),
            afterSend: async () =>
            {
                var status = category.IsActive ? "غیرفعال" : "فعال";
                AddSuccessToast($"دسته‌بندی با موفقیت {status} شد.");
                await LoadCategoriesAsync();
            }
        );
    }

    private async Task OnDeletePost(BlogPostTitleResponse post)
    {
        var result = await DialogService.ShowMessageBox("حذف پست",
            $"آیا برای حذف پست {post.Title} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IBlogPostService>(
                action: (s, ct) => s.DeleteAsync(post.Id, ct),
                afterSend: async () =>
                {
                    AddSuccessToast("پست با موفقیت حذف شد.");
                    await LoadCategoriesAsync();
                });
        }
    }

    private async Task OnSetPostStatus(BlogPostTitleResponse post)
    {
        await SendRequestAsync<IBlogPostService>(
            action: (s, ct) => s.SetStatusAsync(post.Id, !post.IsActive, ct),
            afterSend: async () =>
            {
                var status = post.IsActive ? "غیرفعال" : "فعال";
                AddSuccessToast($"پست با موفقیت {status} شد.");
                await LoadCategoriesAsync();
            }
        );
    }
}