using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Domain.ProductAggregate;

public class MoltenGold : EntityBase
{
    public string? AssayNumber { get; private set; }
    public DateTime? AssayDate { get; private set; }

    public CustomerId? AssayerId { get; private set; }
    public Customer? Assayer { get; private set; }

    public static MoltenGold Create(string? assayNumber, DateTime? assayDate, CustomerId? assayerId)
    {
        return new MoltenGold
        {
            AssayNumber = assayNumber,
            AssayerId = assayerId,
            AssayDate = assayDate
        };
    }
}