using GoldEx.Shared.DTOs.Blogs.BlogCategories;

namespace GoldEx.Shared.Services.Abstractions;

public interface IBlogCategoryService
{
    Task<List<BlogCategoryResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(BlogCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, BlogCategoryRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}