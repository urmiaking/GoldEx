using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Shared.DTOs.LedgerAccounts;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class LedgerAccountMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LedgerAccount, GetLedgerAccountResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.ParentAccount, src => src.ParentAccount)
            .Map(dest => dest.IsSystemAccount, src => src.IsSystemAccount)
            .Map(dest => dest.AccountType, src => src.AccountType);
    }
}