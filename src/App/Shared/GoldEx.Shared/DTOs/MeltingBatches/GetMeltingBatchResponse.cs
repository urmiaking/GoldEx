using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.MeltingBatches;

public record GetMeltingBatchResponse(Guid Id,
    string Description,
    decimal TotalWeight,
    GoldUnitType WeightUnitType,
    MeltingBatchStatus CurrentStatus,
    DateTime CurrentDateTime,
    List<GetProductResponse> Products);