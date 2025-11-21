namespace GoldEx.Shared.DTOs.Blogs.BlogPosts;

public record BlogPostRequest(Guid? Id, Guid CategoryId, string Title, string Slug, string Content);