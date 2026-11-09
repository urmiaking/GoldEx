using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByMoltenGoldTradeSpecification(Guid id) : SpecificationBase<Transaction>(x => x.MoltenGoldTradeId == id);
