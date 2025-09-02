using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class FinancialAccountsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FinancialAccount, GetFinancialAccountResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.FinancialAccountType, src => src.AccountType)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit)
            .Map(dest => dest.LocalBankAccount, src => src.LocalAccount)
            .Map(dest => dest.InternationalBankAccount, src =>
                src.InternationalAccount);

        config.NewConfig<LocalBankAccount, GetLocalBankAccountResponse>();
        config.NewConfig<InternationalBankAccount, GetInternationalBankAccountResponse>();

        config.NewConfig<FinancialAccount, GetFinancialAccountTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src =>
                src.PriceUnit != null
                    ? $"{src.AccountType.GetDisplayName()} - " +
                      $"{(src.AccountType == FinancialAccountType.Cash
                          ? src.CashAccount != null 
                              ? src.CashAccount.AccountType == CashAccountType.Internal 
                                  ? $"{src.CashAccount.AccountType.GetDisplayName()} - {src.PriceUnit.Title}" 
                                  : $"{src.CashAccount.AccountType.GetDisplayName()} - {src.PriceUnit.Title} - {src.BrokerName}"
                              : string.Empty
                          : src.AccountType == FinancialAccountType.LocalBankAccount && src.LocalAccount != null
                              ? src.LocalAccount.AccountNumber
                              : src.AccountType == FinancialAccountType.InternationalBankAccount && src.InternationalAccount != null
                                  ? src.InternationalAccount.AccountNumber
                                  : string.Empty)}" : string.Empty);
    }
}