using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class CustomerRemainingVm
{
    public decimal Amount { get; set; }
    public string PriceUnit { get; set; } = string.Empty;

    public static CustomerRemainingVm CreateFrom(GetCustomerRemainingResponse response)
    {
        return new CustomerRemainingVm
        {
            Amount = response.Amount,
            PriceUnit = response.PriceUnit.Title
        };
    }
}