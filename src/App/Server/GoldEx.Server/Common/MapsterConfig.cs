using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SettingsAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Transactions;
using Mapster;

namespace GoldEx.Server.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Price

        config.NewConfig<Price, GetPriceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Value, src => src.PriceHistory.CurrentValue.ToString("N0"))
            .Map(dest => dest.Change, src => src.PriceHistory.DailyChangeRate)
            .Map(dest => dest.LastUpdate, src => src.PriceHistory.LastUpdate)
            .Map(dest => dest.Unit, src => src.PriceHistory.Unit)
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.IconFileBase64, src => src.IconFile);

        #endregion

        #region Product

        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.Value)
            .Map(dest => dest.ProductCategoryTitle, src => src.ProductCategory.Title)
            .Map(dest => dest.GemStones, src => src.GemStones);

        config.NewConfig<GemStone, GetGemStoneResponse>();

        config.NewConfig<Product, GetPendingProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.Value)
            .Map(dest => dest.IsDeleted, src => src.IsDeleted)
            .Map(dest => dest.GemStones, src => src.GemStones);

        #endregion

        #region ProductCategory

        config.NewConfig<ProductCategory, GetCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<ProductCategory, GetPendingCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);


        #endregion

        #region Settings

        config.NewConfig<Settings, GetSettingsResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Customers

        config.NewConfig<Customer, GetCustomerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<Customer, GetPendingCustomerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Transactions

        config.NewConfig<Transaction, GetTransactionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<Transaction, GetPendingTransactionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerId, src => src.CustomerId.Value);

        #endregion
    }
}
