using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.MeltingBatchAggregate;

public class MeltingBatchChangeLog : EntityBase
{
    public DateTime DateTime { get; private set; }
    public string? Description { get; private set; }
    public MeltingBatchStatus Status { get; private set; }

    private MeltingBatchChangeLog(MeltingBatchStatus status, string? description)
    {
        DateTime = DateTime.Now;
        Description = description;
        Status = status;
    }

    public static MeltingBatchChangeLog Create(MeltingBatchStatus status, string? description = null)
    {
        return new MeltingBatchChangeLog(status, description);
    }
}