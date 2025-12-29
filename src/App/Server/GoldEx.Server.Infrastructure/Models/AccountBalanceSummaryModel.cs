namespace GoldEx.Server.Infrastructure.Models;

public record AccountBalanceSummaryModel(string PriceUnit,
    decimal Debit,
    decimal Credit);