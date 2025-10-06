using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.DTOs.ProductCategories;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class ProductCategoriesMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProductCategory, GetProductCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}