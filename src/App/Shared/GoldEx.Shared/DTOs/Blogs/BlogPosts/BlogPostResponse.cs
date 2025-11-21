namespace GoldEx.Shared.DTOs.Blogs.BlogPosts;

public record BlogPostResponse(Guid Id, string Title, string Slug, string Content, bool IsActive);