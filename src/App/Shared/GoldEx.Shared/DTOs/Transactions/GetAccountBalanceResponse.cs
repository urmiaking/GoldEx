namespace GoldEx.Shared.DTOs.Transactions;

public record GetAccountBalanceResponse(string PriceUnit, decimal Debit, decimal Credit);