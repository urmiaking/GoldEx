using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BlogPostAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Blogs;

public class BlogPostsDefaultSpecification : SpecificationBase<BlogPost>
{
    public BlogPostsDefaultSpecification()
    {
        ApplyOrderBy(x => x.Title);
    }
}

public class BlogPostsByIdSpecification(BlogPostId id) : SpecificationBase<BlogPost>(x => x.Id == id);

public class BlogPostsBySlugSpecification(string slug) : SpecificationBase<BlogPost>(x => x.Slug == slug);