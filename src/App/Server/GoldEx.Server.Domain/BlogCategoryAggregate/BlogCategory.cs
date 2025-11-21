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

    public BlogCategory? ParentCategory { get; private set; }
    public BlogCategoryId? ParentCategoryId { get; private set; }

    public IReadOnlyList<BlogCategory>? SubCategories { get; private set; }

    public IReadOnlyList<BlogPost>? BlogPosts { get; private set; }

    private BlogCategory(string title, BlogCategoryId? parentCategoryId = null)
    {
        Id = new BlogCategoryId(Guid.NewGuid());
        Title = title;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

#pragma warning disable CS8618
    private BlogCategory() { }
#pragma warning restore CS8618

    public void SetTitle(string title) => Title = title;
    public void SetParent(BlogCategoryId categoryId) => ParentCategoryId = categoryId;
    public void SetStatus(bool active) => IsActive = active;
}