namespace GoldEx.Server.Infrastructure.Models.Blogs;

public record BlogPostDto
(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    Guid CategoryId,
    DateTime CreatedAt,
    DateTime LastUpdated,
    bool IsActive
);