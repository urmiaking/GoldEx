using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BlogCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Blogs;

public class BlogCategoriesDefaultSpecifications : SpecificationBase<BlogCategory>
{
    public BlogCategoriesDefaultSpecifications(bool? isActive)
    {
        ApplyOrderBy(x => x.Title);
        AddInclude(x => x.BlogPosts!);

        if (isActive.HasValue)
        {
            AddCriteria(x => x.IsActive == isActive.Value);
        }
    }
}

public class BlogCategoriesByIdSpecifications(BlogCategoryId id) : SpecificationBase<BlogCategory>(x => x.Id == id);