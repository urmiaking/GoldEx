using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public record InventoryWeightChartData(
    string Label,
    decimal Weight,
    GoldUnitType TargetUnit
);