namespace GoldEx.Server.Domain.FinancialAccountAggregate;

public class InternationalBankAccount
{
    public static InternationalBankAccount Create(
        string accountHolderName,
        string bankName,
        string swiftBicCode,
        string ibanNumber,
        string accountNumber)
    {
        return new InternationalBankAccount
        {
            AccountHolderName = accountHolderName,
            BankName = bankName,
            SwiftBicCode = swiftBicCode,
            IbanNumber = ibanNumber,
            AccountNumber = accountNumber
        };
    }
    public string AccountHolderName { get; private set; }
    public string BankName { get; private set; }
    public string SwiftBicCode { get; private set; }
    public string IbanNumber { get; private set; }
    public string AccountNumber { get; private set; }

#pragma warning disable CS8618
    private InternationalBankAccount() { }
#pragma warning restore CS8618
}