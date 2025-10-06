using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByFinancialAccountTypeSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByFinancialAccountTypeSpecification(FinancialAccountType? financialAccountType)
    {
        AddInclude(x => x.ParentAccount!);
        // TODO: this has to be fixed
        if (financialAccountType != null)
        {
            switch (financialAccountType.Value)
            {
                case FinancialAccountType.LocalBankAccount:
                    AddCriteria(x => x.ParentAccount!.Title == SystemLedgerAccounts.Banks || x.Title == SystemLedgerAccounts.Banks);
                    break;
                case FinancialAccountType.InternationalBankAccount:
                    AddCriteria(x => x.ParentAccount!.Title == SystemLedgerAccounts.Banks || x.Title == SystemLedgerAccounts.Banks);
                    break;
                case FinancialAccountType.Cash:
                    AddCriteria(x => x.ParentAccount!.Title == SystemLedgerAccounts.CashAccounts || x.Title == SystemLedgerAccounts.CashAccounts);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}