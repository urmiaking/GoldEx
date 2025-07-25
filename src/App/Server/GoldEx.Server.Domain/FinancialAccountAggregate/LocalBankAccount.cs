namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public class LocalBankAccount
{
    public static LocalBankAccount Create(
        string accountHolderName,
        string bankName,
        string cardNumber,
        string shabaNumber,
        string accountNumber)
    {
        return new LocalBankAccount
        {
            AccountHolderName = accountHolderName,
            BankName = bankName,
            CardNumber = cardNumber,
            ShabaNumber = shabaNumber,
            AccountNumber = accountNumber
        };
    }

    public string AccountHolderName { get; private set; }
    public string BankName { get; private set; }
    public string CardNumber { get; private set; }
    public string ShabaNumber { get; private set; }
    public string AccountNumber { get; private set; }

#pragma warning disable CS8618
    private LocalBankAccount() { }
#pragma warning restore CS8618
}