namespace GoldEx.Shared.DTOs.Customers;

public class GetCustomerOverviewResponse
{
    public int TotalCustomersCount { get; set; }
    public int RetailCustomersCount { get; set; }
    public int WholesaleCustomersCount { get; set; }
}
