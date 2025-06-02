using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.ProductCategories;
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
            .Map(dest => dest.Value,
                src => src.PriceHistory != null ? src.PriceHistory.CurrentValue.ToString("N0") : "-")
            .Map(dest => dest.Change, src => src.PriceHistory != null ? src.PriceHistory.DailyChangeRate : "-")
            .Map(dest => dest.LastUpdate, src => src.PriceHistory != null ? src.PriceHistory.LastUpdate : "-")
            .Map(dest => dest.Unit, src => src.PriceHistory != null ? src.PriceHistory.Unit : "-")
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceHistoryIconExists(src.Id.Value));

        #endregion

        #region Product

        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.HasValue ? src.ProductCategoryId.Value.Value : (Guid?)null)
            .Map(dest => dest.ProductCategoryTitle, src => src.ProductCategory != null ? src.ProductCategory.Title : null)
            .Map(dest => dest.GemStones, src => src.GemStones);

        config.NewConfig<GemStone, GetGemStoneResponse>();

        #endregion

        #region ProductCategory

        config.NewConfig<ProductCategory, GetProductCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Settings

        config.NewConfig<Setting, GetSettingResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Customers

        config.NewConfig<Customer, GetCustomerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Transactions

        config.NewConfig<Transaction, GetTransactionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion
    }
}
