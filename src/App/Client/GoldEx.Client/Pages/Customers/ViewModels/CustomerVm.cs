using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class CustomerVm
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = default!;
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public string NationalId { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Address { get; set; }
    public decimal? CreditLimit { get; set; }
    public GetPriceUnitTitleResponse? CreditLimitPriceUnit { get; set; }
    public bool PriceUnitHasIcon { get; set; }

    internal static CustomerVm CreateFrom(GetCustomerResponse response)
    {
        return new CustomerVm
        {
            Id = response.Id,
            FullName = response.FullName,
            CustomerType = response.CustomerType,
            NationalId = response.NationalId,
            PhoneNumber = response.PhoneNumber,
            Address = response.Address,
            CreditLimit = response.CreditLimit,
            CreditLimitPriceUnit = response.CreditLimitPriceUnit,
            PriceUnitHasIcon = response.CreditLimitPriceUnit?.HasIcon ?? false
        };
    }

    public static CustomerVm CreateDefaultInstance()
    {
        return new CustomerVm();
    }

    public static CustomerRequestDto ToCreateRequest(CustomerVm model)
    {
        return new CustomerRequestDto(null,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitPriceUnit?.Id,
            model.CustomerType);
    }

    public static CustomerRequestDto ToUpdateRequest(CustomerVm model)
    {
        return new CustomerRequestDto(model.Id,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitPriceUnit?.Id,
            model.CustomerType);
    }
}