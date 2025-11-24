using GoldEx.Server.Infrastructure.Helpers;
using GoldEx.Server.Infrastructure.Models.Blogs;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace GoldEx.Server.Infrastructure.Repositories;

internal class BlogFileRepository(string basePath) : IBlogFileRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true // Useful for debugging, set to false for slight performance boost
    };

    private string PostPath(Guid id) => Path.Combine(basePath, "posts", $"{id}.json");
    private string CategoryPath(Guid id) => Path.Combine(basePath, "categories", $"{id}.json");

    // ---------------------------
    // POSTS
    // ---------------------------
    public async Task SavePostAsync(BlogPostDto dto, CancellationToken cancellationToken = default)
    {
        var path = PostPath(dto.Id);
        await FileHelpers.AtomicWriteAsync(path, dto, _jsonOptions, cancellationToken);
    }

    public Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var path = PostPath(postId);
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch (IOException) { /* Ignore if locked/missing */ }
        }
        return Task.CompletedTask;
    }

    public async Task<BlogPostDto?> ReadPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var path = PostPath(postId);
        return await FileHelpers.ReadWithRetriesAsync<BlogPostDto>(path, _jsonOptions, cancellationToken);
    }

    public async Task<IEnumerable<BlogPostDto>> ReadAllPostsAsync(CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(basePath, "posts");
        if (!Directory.Exists(dir)) return Enumerable.Empty<BlogPostDto>();

        var files = Directory.GetFiles(dir, "*.json");
        var results = new ConcurrentBag<BlogPostDto>();

        // OPTIMIZATION: Read files in parallel
        // This makes a huge difference when sync starts
        await Parallel.ForEachAsync(files,
            new ParallelOptions { MaxDegreeOfParallelism = 20, CancellationToken = cancellationToken },
            async (file, ct) =>
            {
                var dto = await FileHelpers.ReadWithRetriesAsync<BlogPostDto>(file, _jsonOptions, ct);
                if (dto != null) results.Add(dto);
            });

        return results;
    }

    // ---------------------------
    // CATEGORIES
    // ---------------------------
    public async Task SaveCategoryAsync(BlogCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var path = CategoryPath(dto.Id);
        await FileHelpers.AtomicWriteAsync(path, dto, _jsonOptions, cancellationToken);
    }

    public async Task<BlogCategoryDto?> ReadCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var path = CategoryPath(categoryId);
        return await FileHelpers.ReadWithRetriesAsync<BlogCategoryDto>(path, _jsonOptions, cancellationToken);
    }

    public async Task<IEnumerable<BlogCategoryDto>> ReadAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(basePath, "categories");
        if (!Directory.Exists(dir)) return Enumerable.Empty<BlogCategoryDto>();

        var files = Directory.GetFiles(dir, "*.json");
        var results = new ConcurrentBag<BlogCategoryDto>();

        await Parallel.ForEachAsync(files,
            new ParallelOptions { MaxDegreeOfParallelism = 20, CancellationToken = cancellationToken },
            async (file, ct) =>
            {
                var dto = await FileHelpers.ReadWithRetriesAsync<BlogCategoryDto>(file, _jsonOptions, ct);
                if (dto != null) results.Add(dto);
            });

        return results;
    }
}