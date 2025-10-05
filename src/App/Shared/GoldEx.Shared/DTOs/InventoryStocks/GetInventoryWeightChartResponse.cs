using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.InventoryStocks;

public record GetInventoryWeightChartResponse(string Label, decimal Weight, GoldUnitType TargetUnit);