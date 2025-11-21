using GoldEx.Client.Pages.Blogs.ViewModels;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Blogs.Components;

public partial class BlogPostItem
{
    private BlogPostVm? _blogPost;
    private BlogPostVm _newPostModel = new();
    private Guid? _lastLoadedPostId;

    [Parameter] public BlogPostTitleResponse? Item { get; set; }
    [Parameter] public Guid? SelectedCategoryId { get; set; }
    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public bool IsCreate { get; set; }
    [Parameter] public EventCallback OnPostSaved { get; set; }

    private bool IsLoading => Item is not null && _blogPost is null && !IsCreate;

    protected override async Task OnParametersSetAsync()
    {
        if (Item?.Id != _lastLoadedPostId)
        {
            _blogPost = null;
        }

        if (IsCreate && SelectedCategoryId.HasValue)
        {
            _blogPost = null;
            _newPostModel = new BlogPostVm
            {
                CategoryId = SelectedCategoryId.Value,
            };
            return;
        }

        await LoadBlogPostAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadBlogPostAsync()
    {
        if (Item is null) return;

        // Prevent duplicate calls
        if (_blogPost != null && _blogPost.Id == Item.Id) return;

        await SendRequestAsync<IBlogPostService, BlogPostResponse>(
            action: (s, ct) => s.GetAsync(Item.Id, ct),
            afterSend: response =>
            {
                _blogPost = BlogPostVm.CreateFrom(response);
                _lastLoadedPostId = Item.Id;
            });
    }

    private string GetHeaderTitle()
    {
        if (IsCreate) return "ایجاد پست جدید";
        if (IsEdit) return "ویرایش پست";
        return _blogPost?.Title ?? "در حال بارگذاری...";
    }

    private async Task HandlePostSaved()
    {
        IsEdit = false;
        IsCreate = false;

        _blogPost = null;

        await LoadBlogPostAsync();

        if (OnPostSaved.HasDelegate)
        {
            await OnPostSaved.InvokeAsync();
        }

        StateHasChanged();
    }
    private void HandleEditCancelled()
    {
        IsEdit = false;
        IsCreate = false;
    }
}