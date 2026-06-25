namespace GoldEx.Shared.Enums;

public enum CheckCashingAdjustmentMode
{
    FixedRate = 1, // Store absorbs the price difference (Exchange Gain/Loss)
    DailyRate = 2  // Customer ledger is adjusted for the gold weight difference
}
