using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.Reporting;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class ReportingMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LedgerAccountTrialBalanceNodeModel, LedgerAccountTrialBalanceRpResponse>();
    }
}