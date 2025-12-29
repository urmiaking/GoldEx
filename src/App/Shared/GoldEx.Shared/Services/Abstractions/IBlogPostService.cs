using GoldEx.Shared.DTOs.Blogs.BlogPosts;

namespace GoldEx.Shared.Services.Abstractions;

public interface IBlogPostService
{
    Task<BlogPostResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlogPostResponse?> GetAsync(string slug, CancellationToken cancellationToken = default);
    Task CreateAsync(BlogPostRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, BlogPostRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string slug, CancellationToken cancellationToken = default);
}