using GoldEx.Server.Domain.BlogCategoryAggregate;
using GoldEx.Shared.DTOs.Blogs.BlogCategories;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class BlogCategoryMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BlogCategory, BlogCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ParentCategoryId,
                src => src.ParentCategoryId.HasValue
                    ? src.ParentCategoryId.Value.Value
                    : (Guid?)null)
            .Map(dest => dest.Posts,
                src => src.BlogPosts != null ? src.BlogPosts.OrderBy(p => p.Title) : null);
    }
}