using GoldEx.Server.Infrastructure.Models.Blogs;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBlogFileRepository
{
    Task SavePostAsync(BlogPostDto dto, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<BlogPostDto?> ReadPostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPostDto>> ReadAllPostsAsync(CancellationToken cancellationToken = default);
    Task SaveCategoryAsync(BlogCategoryDto dto, CancellationToken cancellationToken = default);
    Task<BlogCategoryDto?> ReadCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogCategoryDto>> ReadAllCategoriesAsync(CancellationToken cancellationToken = default);
}