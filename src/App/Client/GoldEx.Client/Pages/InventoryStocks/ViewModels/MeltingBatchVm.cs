using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.InventoryStocks.ViewModels;

public class MeltingBatchVm
{
    public Guid? Id { get; set; }
    public decimal? TotalWeight { get; set; }
    public GoldUnitType WeightUnitType { get; set; }

    [Display(Name = "نام آزمایشگاه")]
    public GetCustomerNameResponse? Assayer { get; set; }

    [Display(Name = "یادداشت")]
    public string? LabDescription { get; set; }

    [Display(Name = "وزن طلای آبشده")]
    public decimal? MeltedGoldWeight { get; set; }

    [Display(Name = "عیار نهایی")] 
    public decimal? Fineness { get; set; } = 750m;

    [Display(Name = "شماره انگ")]
    public string? AssayNumber { get; set; }

    [Display(Name = "یادداشت های تکمیلی")]
    public string? CompleteDescription { get; set; }

    [Display(Name = "نرخ گرم")]
    public decimal? GramPrice { get; set; }

    [Display(Name = "هزینه ذوب/ری گیری")]
    public decimal? FeeAmount { get; set; }

    public GetPriceUnitTitleResponse? FeePriceUnit { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? FeeExchangeRate { get; set; }

    public List<ProductVm> Products { get; set; } = [];

    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    public decimal? WasteWeight => TotalWeight - MeltedGoldWeight;

    public decimal? WastePercentage => TotalWeight.HasValue && WasteWeight.HasValue && TotalWeight != 0
        ? (WasteWeight / TotalWeight) * 100
        : null;

    [Display(Name = "ارزش طلای آبشده")]
    public decimal? MeltedGoldPrice => GramPrice.HasValue && MeltedGoldWeight.HasValue && Fineness.HasValue
        ? CalculatorHelper.MoltenGold.Calculate(MeltedGoldWeight.Value, Fineness.Value, GramPrice.Value, null)
        : null;

    [Display(Name = "ارزش ذوب/ری گیری")]
    public decimal? FeePrice => FeeAmount.HasValue && FeePriceUnit != null
        ? (FeeExchangeRate ?? 1) * FeeAmount.Value
        : null;

    [Display(Name = "ارزش کل")]
    public decimal? TotalPrice => MeltedGoldPrice + (FeePrice ?? 0m);

    [Display(Name = "صندوق پرداختی")]
    public GetFinancialAccountTitleResponse? FinancialAccount { get; set; }

    public static MeltingBatchVm CreateFrom(GetMeltingBatchResponse response)
    {
        return new MeltingBatchVm
        {
            Id = response.Id,
            TotalWeight = response.TotalWeight,
            WeightUnitType = response.WeightUnitType,
            Products = response.Products.Select(ProductVm.CreateFrom).ToList(),
            Assayer = response.Assayer
        };
    }

    public (GoldUnitType selectedUnit, decimal totalWeight) GetWeightParams(decimal? gramPerMesghal)
    {
        decimal? totalWeight;
        GoldUnitType selectedUnit;

        var gramBasedProducts = Products.Where(p => p.GoldUnitType == GoldUnitType.Gram).ToList();
        var mesghalBasedProducts = Products.Where(p => p.GoldUnitType == GoldUnitType.Mesghal).ToList();

        var mesghalToGramFactor = gramPerMesghal ?? 4.6083m;

        if (gramBasedProducts.Count > mesghalBasedProducts.Count)
        {
            selectedUnit = GoldUnitType.Gram;
            var gramWeights = gramBasedProducts.Sum(p => p.Weight);
            var convertedMesghalWeights = mesghalBasedProducts.Sum(p => p.Weight * mesghalToGramFactor);
            totalWeight = gramWeights + convertedMesghalWeights;
        }
        else if (mesghalBasedProducts.Count > gramBasedProducts.Count)
        {
            selectedUnit = GoldUnitType.Mesghal;
            var mesghalWeights = mesghalBasedProducts.Sum(p => p.Weight);
            var convertedGramWeights = gramBasedProducts.Sum(p => p.Weight / mesghalToGramFactor);
            totalWeight = mesghalWeights + convertedGramWeights;
        }
        else
        {
            selectedUnit = GoldUnitType.Gram;
            var gramWeights = gramBasedProducts.Sum(p => p.Weight);
            var convertedMesghalWeights = mesghalBasedProducts.Sum(p => p.Weight * mesghalToGramFactor);
            totalWeight = gramWeights + convertedMesghalWeights;
        }

        return (selectedUnit, totalWeight ?? 0);
    }

    public MeltingBatchRequestDto ToMeltingRequest()
    {
        return new MeltingBatchRequestDto(Products.Select(x => x.Id!.Value).ToList());
    }

    public SendToLabRequestDto ToSendToLabRequest()
    {
        return new SendToLabRequestDto(Assayer!.Id, LabDescription);
    }

    public CompleteMeltingRequestDto ToCompleteMeltingRequest()
    {
        return new CompleteMeltingRequestDto(MeltedGoldWeight!.Value,
            Fineness!.Value,
            AssayNumber!,
            GramPrice!.Value,
            PriceUnit!.Id,
            CompleteDescription,
            FeeAmount,
            FeeExchangeRate,
            FeePriceUnit?.Id,
            FinancialAccount?.Id);
    }
}