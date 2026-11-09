using GoldEx.Shared.DTOs.MoltenGold;

namespace GoldEx.Client.Pages.Invoices.Components;

public enum MoltenGoldOperation
{
    Payment,
    Delivery,
    Transfer,
    GoldSettlement,
    Cancel
}

public sealed record MoltenGoldDeliveryDraft(decimal Weight, string? AssayNumber, MoltenGoldStockChoice? Stock);
