using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryEntries;

public record CreateProductItemRequest(
    ProductType Type,
    string? Barcode,
    string? Name,
    string? Category,
    decimal Fineness,
    WageType? WageType,
    decimal Wage,
    string? WagePriceUnitTitle,
    CreateMoltenGoldItemRequest? MoltenGold);