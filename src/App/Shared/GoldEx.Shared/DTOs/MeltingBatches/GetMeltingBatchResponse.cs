using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.MeltingBatches;

public record GetMeltingBatchResponse(Guid Id,
    int BatchNumber,
    decimal TotalWeight,
    GoldUnitType WeightUnitType,
    MeltingBatchStatus CurrentStatus,
    DateTime CurrentDateTime,
    GetCustomerNameResponse? Assayer,
    GetMeltingBatchDetailResponse? FinalProductDetail,
    List<GetMeltingBatchTransactionSummary>? Transactions,
    List<GetProductResponse> Products);