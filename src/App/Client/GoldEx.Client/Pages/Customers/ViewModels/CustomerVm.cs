using GoldEx.Shared.DTOs.Customers;
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
    public double? CreditLimit { get; set; }
    public UnitType? CreditLimitUnit { get; set; }

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
            CreditLimitUnit = response.CreditLimitUnit
        };
    }

    public static CustomerVm CreateDefaultInstance()
    {
        return new CustomerVm();
    }

    public static CreateCustomerRequest ToCreateRequest(CustomerVm model)
    {
        return new CreateCustomerRequest(Guid.NewGuid(),
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitUnit,
            model.CustomerType);
    }

    public static UpdateCustomerRequest ToUpdateRequest(CustomerVm model)
    {
        return new UpdateCustomerRequest(model.Id,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitUnit,
            model.CustomerType);
    }
}