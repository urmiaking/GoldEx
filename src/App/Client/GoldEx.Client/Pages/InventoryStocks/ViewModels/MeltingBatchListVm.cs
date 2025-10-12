using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.InventoryStocks.ViewModels;

public class MeltingBatchListVm
{
    public Guid Id { get; set; }
    public int BatchNumber { get; set; }
    public decimal TotalWeight { get; set; }
    public GoldUnitType WeightUnitType { get; set; }
    public MeltingBatchStatus CurrentStatus { get; set; }
    public DateTime CurrentDateTime { get; set; }
    public bool ShowDetails { get; set; }

    public GetCustomerNameResponse? Assayer { get; set; }
    public List<GetProductResponse> Products { get; set; } = [];
    public GetMeltingBatchDetailResponse? FinalProductDetail { get; set; }

    public decimal? WasteWeight => TotalWeight - FinalProductDetail?.Weight;
    public decimal? WastePercentage => WasteWeight.HasValue && TotalWeight != 0
        ? (WasteWeight / TotalWeight) * 100
        : null;

    public static MeltingBatchListVm CreateFrom(GetMeltingBatchResponse response)
    {
        return new MeltingBatchListVm
        {
            Id = response.Id,
            TotalWeight = response.TotalWeight,
            BatchNumber = response.BatchNumber,
            WeightUnitType = response.WeightUnitType,
            CurrentStatus = response.CurrentStatus,
            CurrentDateTime = response.CurrentDateTime,
            Assayer = response.Assayer,
            Products = response.Products,
            FinalProductDetail = response.FinalProductDetail
        };
    }
}