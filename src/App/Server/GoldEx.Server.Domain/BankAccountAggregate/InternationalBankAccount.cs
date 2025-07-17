namespace GoldEx.Server.Domain.BankAccountAggregate;

public class InternationalBankAccount
{
    public static InternationalBankAccount Create(
        string swiftBicCode,
        string ibanNumber,
        string accountNumber)
    {
        return new InternationalBankAccount
        {
            SwiftBicCode = swiftBicCode,
            IbanNumber = ibanNumber,
            AccountNumber = accountNumber
        };
    }

    public string SwiftBicCode { get; private set; }
    public string IbanNumber { get; private set; }
    public string AccountNumber { get; private set; }

#pragma warning disable CS8618
    private InternationalBankAccount() { }
#pragma warning restore CS8618
}