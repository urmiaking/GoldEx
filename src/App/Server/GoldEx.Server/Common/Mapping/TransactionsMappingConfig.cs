using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Transactions;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class TransactionsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Transaction, GetTransactionResponse>()
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.TransactionType, src => src.TransactionType)
            .Map(dest => dest.LedgerAccount, src => src.LedgerAccount != null ? src.LedgerAccount.Title : string.Empty)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}