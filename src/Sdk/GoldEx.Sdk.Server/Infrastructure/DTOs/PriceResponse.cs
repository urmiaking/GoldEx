﻿using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Sdk.Server.Infrastructure.DTOs;

public record PriceResponse(
    string Title,
    double CurrentValue,
    string Unit,
    string LastUpdate,
    string Change,
    string? IconUrl,
    MarketType MarketType);