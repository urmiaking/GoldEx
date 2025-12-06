using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.DTOs.FinancialAccounts;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class FinancialAccountMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FinancialAccount, GetFinancialAccountResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.FinancialAccountType, src => src.AccountType)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit)
            .Map(dest => dest.LocalBankAccount, src => src.LocalAccount)
            .Map(dest => dest.InternationalBankAccount, src => src.InternationalAccount)
            .Map(dest => dest.CashAccount, src => src.CashAccount)
            .Map(dest => dest.BrokerName, src => src.BrokerName)
            .Map(dest => dest.HolderName, src => src.HolderName);

        config.NewConfig<LocalBankAccount, GetLocalBankAccountResponse>();
        config.NewConfig<InternationalBankAccount, GetInternationalBankAccountResponse>();

        config.NewConfig<FinancialAccount, GetFinancialAccountTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => BuildTitle(src));
    }

    private static string BuildTitle(FinancialAccount src)
    {
        return src.GetAccountTypeText();
    }
}