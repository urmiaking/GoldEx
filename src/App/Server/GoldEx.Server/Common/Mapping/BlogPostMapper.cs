using GoldEx.Server.Domain.BlogPostAggregate;
using GoldEx.Shared.DTOs.Blogs.BlogPosts;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class BlogPostMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BlogPost, BlogPostResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<BlogPost, BlogPostTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}