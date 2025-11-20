using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.DTOs.Customers;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class CustomerMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, GetCustomerResponse>()
            .PreserveReference(true)
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CreditLimitPriceUnit, src => src.CreditLimitPriceUnit)
            .Map(dest => dest.FinancialAccounts, src => src.FinancialAccounts);

        config.NewConfig<Customer, GetCustomerNameResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}