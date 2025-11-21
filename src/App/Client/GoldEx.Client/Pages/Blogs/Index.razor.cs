using GoldEx.Client.Pages.Blogs.Components;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Blogs;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new ("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new ("راهنما", href: ClientRoutes.Blogs.Index, icon: Icons.Material.Filled.Help)
    ];
    private readonly int _jsVersion = new Random().Next(0, 1000);
    private BlogPostTitleResponse? _selectedPost;
    private bool _isEditMode;
    private bool _isCreateMode;
    private Guid? _selectedCategoryId;
    private BlogCategories _blogCategoriesRef = null!;

    private void HandlePostSelection(BlogPostTitleResponse item)
    {
        _selectedPost = item;

        _isEditMode = false;
        _isCreateMode = false;
    }

    private void HandlePostEdit(BlogPostTitleResponse item)
    {
        _isEditMode = true;
        _isCreateMode = false;
        _selectedPost = item;
    }

    private void HandlePostCreate(BlogCategoryResponse item)
    {
        _isCreateMode = true;
        _isEditMode = false;
        _selectedPost = null;
        _selectedCategoryId = item.Id;
    }

    private async Task ReloadCategories()
    {
        await _blogCategoriesRef.LoadCategoriesAsync();
        _selectedPost = null;
        _isEditMode = false;
        _isCreateMode = false;
        StateHasChanged();
    }
}