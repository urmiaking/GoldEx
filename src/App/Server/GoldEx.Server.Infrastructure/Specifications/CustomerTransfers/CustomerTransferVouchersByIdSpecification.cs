using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CustomerTransfers;

public class CustomerTransferVouchersByIdSpecification : SpecificationBase<CustomerTransferVoucher>
{
    public CustomerTransferVouchersByIdSpecification(CustomerTransferVoucherId id)
    {
        AddCriteria(x => x.Id == id);
        AddInclude(x => x.SourceCustomer!);
        AddInclude(x => x.DestinationCustomer!);
        AddInclude(x => x.PriceUnit!);
        AddInclude(x => x.SourceInvoice!);
        AddInclude(x => x.DestinationInvoice!);
    }
}
