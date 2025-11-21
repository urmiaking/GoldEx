using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;

namespace GoldEx.Client.Pages.Blogs.ViewModels;

public class BlogPostVm
{
    public Guid? Id { get; set; }

    public Guid CategoryId { get; set; }

    [Display(Name = "اسلاگ")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "محتوا")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Content { get; set; } = string.Empty;

    public BlogPostRequest ToRequest()
    {
        return new BlogPostRequest(Id, CategoryId, Title, Slug, Content);
    }

    public static BlogPostVm CreateFrom(BlogPostResponse response)
    {
        return new BlogPostVm
        {
            Id = response.Id,
            CategoryId = response.BlogCategoryId,
            Title = response.Title,
            Slug = response.Slug,
            Content = response.Content
        };
    }
}