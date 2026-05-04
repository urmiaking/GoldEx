using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.CheckPaymentAggregate;

public readonly record struct CheckPaymentId(Guid Value);
public class CheckPayment : EntityBase<CheckPaymentId>
{
    private CheckPayment(InvoicePaymentId invoicePaymentId,
        CustomerId issuerId,
        FinancialAccountId issuerFinancialAccountId,
        string? number,
        string? sayadiCode,
        DateTime dueDate)
    {
        if (string.IsNullOrEmpty(sayadiCode) && string.IsNullOrEmpty(number))
            throw new InvalidOperationException("At least sayadi code or check number must be provided");

        Id = new CheckPaymentId(Guid.CreateVersion7());
        InvoicePaymentId = invoicePaymentId;
        IssuerId = issuerId;
        IssuerFinancialAccountId = issuerFinancialAccountId;
        Number = number;
        SayadiCode = sayadiCode;
        DueDate = dueDate;
        _changeLogs = [CheckPaymentChangeLog.Create(CheckPaymentStatus.Pending)];
    }

    private CheckPayment() { }

    public InvoicePaymentId InvoicePaymentId { get; private set; }
    public InvoicePayment? InvoicePayment { get; private set; }

    public CustomerId IssuerId { get; private set; }
    public Customer? Issuer { get; private set; }

    public FinancialAccountId IssuerFinancialAccountId { get; private set; }
    public FinancialAccount? IssuerFinancialAccount { get; private set; }

    private readonly List<CheckPaymentChangeLog> _changeLogs = [];
    public IReadOnlyList<CheckPaymentChangeLog> ChangeLogs => _changeLogs.OrderBy(x => x.DateTime).ToList();

    public CheckPaymentStatus CurrentStatus => ChangeLogs.Any() ? ChangeLogs.MaxBy(x => x.DateTime)!.Status : CheckPaymentStatus.Pending;
    public DateTime LastModifiedAt => ChangeLogs.Any() ? ChangeLogs.MaxBy(x => x.DateTime)!.DateTime : DateTime.MinValue;

    public string? Number { get; private set; }
    public string? SayadiCode { get; private set; }
    public DateTime DueDate { get; private set; }
    
    public static CheckPayment Create(InvoicePaymentId invoicePaymentId,
        CustomerId issuerId,
        FinancialAccountId issuerFinancialAccountId,
        string? number,
        string? sayadiCode,
        DateTime dueDate)
    {
        return new CheckPayment(invoicePaymentId, issuerId, issuerFinancialAccountId, number, sayadiCode, dueDate);
    }

    public void SetIssuer(CustomerId issuerId) => IssuerId = issuerId;
    public void SetIssuerFinancialAccount(FinancialAccountId issuerFinancialAccountId) => IssuerFinancialAccountId = issuerFinancialAccountId;
    public void SetNumber(string? number) => Number = number;
    public void SetSayadiCode(string? sayadiCode) => SayadiCode = sayadiCode;
    public void SetDueDate(DateTime dueDate) => DueDate = dueDate;

    public void Accept(string? description, FinancialAccountId targetAccountId)
        => AddChangeLog(CheckPaymentStatus.Accepted, description, targetAccountId);

    public void Return(string? description)
        => AddChangeLog(CheckPaymentStatus.Returned, description);

    private CheckPayment AddChangeLog(CheckPaymentStatus status, string? description = null, FinancialAccountId? targetAccountId = null)
    {
        _changeLogs.Add(CheckPaymentChangeLog.Create(status, description, targetAccountId));

        return this;
    }
}