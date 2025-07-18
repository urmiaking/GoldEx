﻿namespace GoldEx.Shared.DTOs.Settings;

public record GetSettingResponse(
    Guid Id,
    string InstitutionName,
    string Address,
    string PhoneNumber,
    decimal TaxPercent,
    decimal GoldProfitPercent,
    decimal JewelryProfitPercent,
    decimal GoldSafetyMarginPercent,
    decimal OldGoldCarat,
    TimeSpan PriceUpdateInterval,
    bool HasIcon);