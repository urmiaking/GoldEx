using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class ProductMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.HasValue ? src.ProductCategoryId.Value.Value : (Guid?)null)
            .Map(dest => dest.ProductCategoryTitle, src => src.ProductCategory != null ? src.ProductCategory.Title : null)
            .Map(dest => dest.WagePriceUnitId, src => src.WagePriceUnitId.HasValue ? src.WagePriceUnitId.Value.Value : (Guid?)null)
            .Map(dest => dest.WagePriceUnitTitle, src => src.WagePriceUnit != null ? src.WagePriceUnit.Title : null)
            .Map(dest => dest.StonePriceUnit, src => src.StonePriceUnit)
            .Map(dest => dest.DateTime, src => src.CreatedAt)
            .Map(dest => dest.GemStones, src => src.GemStones)
            .Map(dest => dest.Weight, src => src.InventoryStocks != null 
                ? src.InventoryStocks.Sum(x => x.ActionType == WarehouseActionType.In ? x.ChangeAmount : -x.ChangeAmount)
                : src.Weight);

        config.NewConfig<GemStone, GetGemStoneResponse>()
            .Map(dest => dest.StoneTypeId, src => src.StoneTypeId.HasValue ? src.StoneTypeId.Value.Value : (Guid?)null)
            .Map(dest => dest.StoneTypeSymbol, src => src.StoneType != null ? src.StoneType.Symbol : null);
        config.NewConfig<MoltenGold, GetMoltenGoldResponse>();
    }
}