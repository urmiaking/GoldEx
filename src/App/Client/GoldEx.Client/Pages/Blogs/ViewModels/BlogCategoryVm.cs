using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;

namespace GoldEx.Client.Pages.Blogs.ViewModels;

public class BlogCategoryVm
{
    public Guid? Id { get; set; }
    public Guid? ParentCategoryId { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = null!;

    public static BlogCategoryVm CreateFrom(BlogCategoryResponse response)
    {
        return new BlogCategoryVm
        {
            Id = response.Id,
            Title = response.Title
        };
    }

    public BlogCategoryRequest ToRequest()
    {
        return new BlogCategoryRequest(Id, Title, ParentCategoryId);
    }
}