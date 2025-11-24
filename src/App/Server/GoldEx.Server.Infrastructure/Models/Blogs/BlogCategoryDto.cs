namespace GoldEx.Server.Infrastructure.Models.Blogs;

public record BlogCategoryDto
(
    Guid Id,
    string Title,
    Guid? ParentCategoryId,
    DateTime CreatedAt,
    DateTime LastUpdated,
    bool IsActive
);