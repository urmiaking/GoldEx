using FluentValidation;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Server.Application.Validators.Blogs;
using GoldEx.Server.Domain.BlogCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Blogs;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class BlogCategoryService(
    IBlogCategoryRepository repository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    BlogCategoryRequestValidator validator) : IBlogCategoryService
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    public async Task<List<BlogCategoryResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        bool? isActive = IsAdminUser() ? null : true;

        var list = await repository
            .Get(new BlogCategoriesDefaultSpecifications(isActive))
            .AsNoTracking()
            .Include(x => x.BlogPosts!)
            .Include(x => x.SubCategories!)
            .ThenInclude(x => x.BlogPosts!)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<BlogCategoryResponse>>(list);
    }

    public async Task<BlogCategoryResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        var item = await repository
            .Get(new BlogCategoriesByIdSpecifications(new BlogCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<BlogCategoryResponse>(item);
    }

    public async Task CreateAsync(BlogCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var category = BlogCategory.Create(request.Title,
            request.ParentCategoryId.HasValue ? new BlogCategoryId(request.ParentCategoryId.Value) : null);

        await repository.CreateAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, BlogCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var category = await repository
            .Get(new BlogCategoriesByIdSpecifications(new BlogCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        category.SetTitle(request.Title);

        await repository.UpdateAsync(category, cancellationToken);
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        var category = await repository
            .Get(new BlogCategoriesByIdSpecifications(new BlogCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
        
        category.SetStatus(isActive);

        await repository.UpdateAsync(category, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!IsAdminUser())
            throw new ForbiddenException();

        var category = await repository
            .Get(new BlogCategoriesByIdSpecifications(new BlogCategoryId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await repository.DeleteAsync(category, cancellationToken);
    }

    private bool IsAdminUser() => _httpContext.User.IsInRole(BuiltinRoles.Administrators);
}