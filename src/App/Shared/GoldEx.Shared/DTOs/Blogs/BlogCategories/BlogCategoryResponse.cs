using GoldEx.Shared.DTOs.Blogs.BlogPosts;

namespace GoldEx.Shared.DTOs.Blogs.BlogCategories;

public record BlogCategoryResponse(Guid Id,
    string Title,
    bool IsActive,
    Guid? ParentCategoryId,
    List<BlogPostTitleResponse>? Posts,
    List<BlogCategoryResponse>? SubCategories);