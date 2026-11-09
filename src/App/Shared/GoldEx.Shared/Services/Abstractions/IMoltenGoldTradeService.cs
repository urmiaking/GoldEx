using GoldEx.Shared.DTOs.MoltenGold;

namespace GoldEx.Shared.Services.Abstractions;

public interface IMoltenGoldTradeService
{
    Task<MoltenGoldDeskSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<List<MoltenGoldStockChoice>> SearchStockAsync(string query, CancellationToken cancellationToken = default);
    Task<List<MoltenGoldSettlementChoice>> SearchSettlementsAsync(Guid id, string query, CancellationToken cancellationToken = default);
    Task<List<MoltenGoldPaymentChoice>> SearchPaymentTradesAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default);
    Task<List<MoltenGoldTradeResponse>> GetListAsync(MoltenGoldTradeFilter filter, int skip, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateMoltenGoldTrade request, CancellationToken cancellationToken = default);
    Task DeliverAsync(Guid id, DeliverMoltenGold request, CancellationToken cancellationToken = default);
    Task PayAsync(Guid id, PayMoltenGold request, CancellationToken cancellationToken = default);
    Task TransferAsync(Guid id, TransferMoltenGold request, CancellationToken cancellationToken = default);
    Task SettleGoldAsync(Guid id, SettleMoltenGold request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
