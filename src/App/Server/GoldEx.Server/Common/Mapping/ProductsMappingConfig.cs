using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class ProductsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.HasValue ? src.ProductCategoryId.Value.Value : (Guid?)null)
            .Map(dest => dest.ProductCategoryTitle, src => src.ProductCategory != null ? src.ProductCategory.Title : null)
            .Map(dest => dest.WagePriceUnitId, src => src.WagePriceUnitId.HasValue ? src.WagePriceUnitId.Value.Value : (Guid?)null)
            .Map(dest => dest.WagePriceUnitTitle, src => src.WagePriceUnit != null ? src.WagePriceUnit.Title : null)
            //.Map(dest => dest.InvoiceId, src => src.SellInvoiceProductItem != null ? src.SellInvoiceProductItem.InvoiceId.Value : (Guid?)null)
            // TODO: Remove this mapping when SellProduct navigation property is removed from Product entity
            .Map(dest => dest.DateTime, src => src.CreatedAt)
            .Map(dest => dest.GemStones, src => src.GemStones);

        config.NewConfig<GemStone, GetGemStoneResponse>();
    }
}