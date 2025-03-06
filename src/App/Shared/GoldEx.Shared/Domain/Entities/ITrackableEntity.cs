using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.Domain.Entities;
public interface ITrackableEntity
{
    ModifyStatus Status { get; } 
}
