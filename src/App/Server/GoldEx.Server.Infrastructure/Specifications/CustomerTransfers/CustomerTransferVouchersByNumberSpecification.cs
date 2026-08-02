using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CustomerTransfers;

public class CustomerTransferVouchersByNumberSpecification : SpecificationBase<CustomerTransferVoucher>
{
    public CustomerTransferVouchersByNumberSpecification(long number)
    {
        AddCriteria(x => x.VoucherNumber == number);
        AddInclude(x => x.SourceCustomer!);
        AddInclude(x => x.DestinationCustomer!);
        AddInclude(x => x.PriceUnit!);
        AddInclude(x => x.SourceInvoice!);
        AddInclude(x => x.DestinationInvoice!);
    }
}
