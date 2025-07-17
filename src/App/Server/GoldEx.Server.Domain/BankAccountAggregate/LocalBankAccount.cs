namespace GoldEx.Server.Domain.BankAccountAggregate;

public class LocalBankAccount
{
    public static LocalBankAccount Create(
        string cardNumber,
        string shabaNumber,
        string accountNumber)
    {
        return new LocalBankAccount
        {
            CardNumber = cardNumber,
            ShabaNumber = shabaNumber,
            AccountNumber = accountNumber
        };
    }

    public string CardNumber { get; private set; }
    public string ShabaNumber { get; private set; }
    public string AccountNumber { get; private set; }

#pragma warning disable CS8618
    private LocalBankAccount() { }
#pragma warning restore CS8618
}