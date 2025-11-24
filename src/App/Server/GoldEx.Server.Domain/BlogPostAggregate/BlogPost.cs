using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.BlogCategoryAggregate;

namespace GoldEx.Server.Domain.BlogPostAggregate;

public readonly record struct BlogPostId(Guid Value);
public class BlogPost : EntityBase<BlogPostId>
{
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime? LastUpdated { get; private set; } = DateTime.Now;

    public BlogCategory? BlogCategory { get; private set; }
    public BlogCategoryId BlogCategoryId { get; private set; }

    private BlogPost(string title,
        string slug,
        string content,
        BlogCategoryId categoryId)
    {
        Id = new BlogPostId(Guid.NewGuid());
        Title = title;
        Content = content;
        Slug = slug;
        BlogCategoryId = categoryId;
        IsActive = true;
    }

#pragma warning disable CS8618
    private BlogPost() { }
#pragma warning restore CS8618

    public static BlogPost Create(string title,
        string slug,
        string content,
        BlogCategoryId categoryId) =>
        new(title, slug, content, categoryId);

    public void UpdateContent(string title, string slug, string content)
    {
        Title = title;
        Slug = slug;
        Content = content;
        LastUpdated = DateTime.Now;
    }

    public void SetStatus(bool isActive)
    {
        IsActive = isActive;
        LastUpdated = DateTime.Now;
    }

    public static BlogPost Hydrate(Guid id, string title, string slug, string
        content, BlogCategoryId categoryId, DateTime createdAt, DateTime lastUpdated, bool
        isActive)
    {
        return new BlogPost
        {
            Id = new BlogPostId(id),
            Title = title,
            Slug = slug,
            Content = content,
            BlogCategoryId = categoryId,
            CreatedAt = createdAt,
            LastUpdated = lastUpdated,
            IsActive = isActive
        };
    }

}