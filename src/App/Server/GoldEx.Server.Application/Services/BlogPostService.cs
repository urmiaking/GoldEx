using FluentValidation;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Blogs;
using GoldEx.Server.Domain.BlogCategoryAggregate;
using GoldEx.Server.Domain.BlogPostAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Blogs;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class BlogPostService(
    IBlogPostRepository repository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment hostEnvironment,
    BlogPostRequestValidator validator) : IBlogPostService
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    public async Task<BlogPostResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new BlogPostsByIdSpecification(new BlogPostId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<BlogPostResponse>(item);
    }

    public async Task<BlogPostResponse?> GetAsync(string slug, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new BlogPostsBySlugSpecification(slug))
            .FirstOrDefaultAsync(cancellationToken);

        return item == null ? null : mapper.Map<BlogPostResponse>(item);
    }

    public async Task CreateAsync(BlogPostRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var post = BlogPost.Create(request.Title, request.Slug, request.Content, new BlogCategoryId(request.CategoryId));

        var content = ReplaceTmpPathsAndMoveFiles(post.Id.Value, request.Content);
        post.UpdateContent(request.Title, request.Slug, content);

        await repository.CreateAsync(post, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, BlogPostRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var post = await repository
            .Get(new BlogPostsByIdSpecification(new BlogPostId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var content = ReplaceTmpPathsAndMoveFiles(post.Id.Value, request.Content);

        post.UpdateContent(request.Title, request.Slug, content);

        await repository.UpdateAsync(post, cancellationToken);
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        var post = await repository
            .Get(new BlogPostsByIdSpecification(new BlogPostId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        post.SetStatus(isActive);

        await repository.UpdateAsync(post, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        var post = await repository
            .Get(new BlogPostsByIdSpecification(new BlogPostId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await repository.DeleteAsync(post, cancellationToken);

        var blogDir = hostEnvironment.GetBlogPostDirectoryPath(post.Id.Value);

        if (Directory.Exists(blogDir))
            Directory.Delete(blogDir, true);
    }

    public Task<bool> ExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return repository.ExistsAsync(new BlogPostsBySlugSpecification(slug), cancellationToken);
    }

    private bool IsAdminUser() => _httpContext.User.IsInRole(BuiltinRoles.Administrators);

    /// <summary>
    /// returns the content with temporary paths replaced with permanent ones
    /// and moves the files from temp to permanent location
    /// </summary>
    /// <param name="blogId"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private string ReplaceTmpPathsAndMoveFiles(Guid blogId, string content)
    {
        var blogDir = hostEnvironment.GetBlogPostDirectoryPath(blogId);
        Directory.CreateDirectory(blogDir);

        var regex = new Regex(@"uploads/content/blogs/temp/([^""'\s>]+)", RegexOptions.IgnoreCase);

        var newContent = regex.Replace(content, match =>
        {
            var fileName = Path.GetFileName(match.Value);
            var tmpPath = Path.Combine(hostEnvironment.GetBlogsTempDirectoryPath(), fileName);
            var newPath = hostEnvironment.GetBlogPostFilePath(blogId, fileName);

            if (File.Exists(tmpPath))
            {
                if (!File.Exists(newPath))
                    File.Move(tmpPath, newPath);
            }

            return $"uploads/content/blogs/{blogId}/{fileName}";
        });

        return newContent;
    }
}