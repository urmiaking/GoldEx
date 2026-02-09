using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.BlogPostAggregate;

namespace GoldEx.Server.Domain.BlogCategoryAggregate;

public readonly record struct BlogCategoryId(Guid Value);
public class BlogCategory : EntityBase<BlogCategoryId>
{
    public static BlogCategory Create(string title, BlogCategoryId? parentCategoryId = null)
    {
        return new BlogCategory(title, parentCategoryId);
    }

    public string Title { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime? LastUpdated { get; private set; } = DateTime.Now;

    public BlogCategory? ParentCategory { get; private set; }
    public BlogCategoryId? ParentCategoryId { get; private set; }

    public IReadOnlyList<BlogCategory>? SubCategories { get; private set; }

    public IReadOnlyList<BlogPost>? BlogPosts { get; private set; }

    private BlogCategory(string title, BlogCategoryId? parentCategoryId = null)
    {
        Id = new BlogCategoryId(Guid.CreateVersion7());
        Title = title;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

#pragma warning disable CS8618
    private BlogCategory() { }
#pragma warning restore CS8618

    public void SetTitle(string title)
    {
        Title = title;
        LastUpdated = DateTime.Now;
    }

    public void SetStatus(bool active)
    {
        IsActive = active;
        LastUpdated = DateTime.Now;
    }

    public static BlogCategory Hydrate(Guid id, string title, BlogCategoryId? parentCategoryId, DateTime createdAt, DateTime lastUpdated)
    {
        var category = new BlogCategory
        {
            Id = new BlogCategoryId(id),
            Title = title,
            LastUpdated = lastUpdated,
            ParentCategoryId = parentCategoryId
        };
        return category;
    }

    public void SetParent(BlogCategoryId? blogCategoryId)
    {
        ParentCategoryId = blogCategoryId;
        LastUpdated = DateTime.Now;
    }
}