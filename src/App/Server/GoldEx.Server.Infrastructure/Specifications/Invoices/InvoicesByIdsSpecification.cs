using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByIdsSpecification(InvoiceId[] ids) : SpecificationBase<Invoice>(x => ids.Contains(x.Id));
