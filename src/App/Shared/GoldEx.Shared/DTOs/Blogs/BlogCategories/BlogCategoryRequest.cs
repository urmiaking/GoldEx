namespace GoldEx.Shared.DTOs.Blogs.BlogCategories;

public record BlogCategoryRequest(Guid? Id, string Title, Guid? ParentCategoryId);