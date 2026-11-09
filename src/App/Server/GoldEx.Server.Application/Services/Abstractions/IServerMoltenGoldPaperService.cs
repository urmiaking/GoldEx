using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerMoltenGoldPaperService
{
    Task PostAgreementAsync(MoltenGoldTrade trade, CancellationToken ct);
    Task ReverseAgreementAsync(MoltenGoldTrade trade, CancellationToken ct);
    Task SettleDeliveryAsync(MoltenGoldTrade trade, Invoice invoice, MoltenGoldPosition position, CancellationToken ct);
    Task<Guid> SettleGoldAsync(MoltenGoldTrade first, MoltenGoldTrade second, decimal goldWeight, CancellationToken ct);
}
