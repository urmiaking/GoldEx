using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.CustomerAggregate;

public readonly record struct CustomerId(Guid Value);
public class Customer : EntityBase<CustomerId>, IStoreFiltered
{
    public static Customer Create(CustomerType customerType, string fullName, string nationalId,
        string? phoneNumber, string? address, decimal? creditLimit, PriceUnitId? creditLimitUnit, StoreId storeId = default)
    {
        return new Customer
        {
            Id = new CustomerId(Guid.CreateVersion7()),
            CustomerType = customerType,
            FullName = fullName,
            NationalId = nationalId,
            PhoneNumber = phoneNumber,
            Address = address,
            CreditLimit = creditLimit,
            CreditLimitPriceUnitId = creditLimitUnit,
            StoreId = storeId
        };
    }

#pragma warning disable CS8618
    private Customer() { }
#pragma warning restore CS8618

    public StoreId StoreId { get; private set; }
    public string FullName { get; private set; }
    public string NationalId { get; private set; }
    public string?  PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public CustomerType CustomerType { get; private set; }

    public PriceUnitId? CreditLimitPriceUnitId { get; private set; }
    public PriceUnit? CreditLimitPriceUnit { get; private set; }

    public IReadOnlyList<LedgerAccount>? LedgerAccounts { get; private set; }

    public IReadOnlyList<FinancialAccount>? FinancialAccounts { get; private set; }
    public IReadOnlyList<PaymentVoucher>? PaymentVouchers { get; private set; }

    public void SetCustomerType(CustomerType customerType) => CustomerType = customerType;
    public void SetFullName(string fullName) => FullName = fullName;
    public void SetNationalId(string nationalId) => NationalId = nationalId;
    public void SetPhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
    public void SetAddress(string? address) => Address = address;
    public void SetCreditLimit(decimal? creditLimit, PriceUnitId? creditLimitUnit)
    {
        CreditLimit = creditLimit;
        CreditLimitPriceUnitId = creditLimitUnit;
    }
}
