using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.MeltingBatches;

public record GetMeltingBatchDetailResponse(GetProductResponse Product, decimal Weight, GoldUnitType WeightUnitType, decimal Fineness, string AssayNumber);