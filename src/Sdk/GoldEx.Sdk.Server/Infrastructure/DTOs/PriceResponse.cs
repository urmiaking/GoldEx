using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Sdk.Server.Infrastructure.DTOs;

public record PriceResponse(
    string Title,
    double CurrentValue, 
    string LastUpdate,
    string DailyChangeRate,
    PriceType PriceType);