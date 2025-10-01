using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.DTOs.FinancialAccounts;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByFilterSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByFilterSpecification(RequestFilter filter, FinancialAccountFilter financialAccountFilter)
    {
        // TODO: since we will use this specification in customer's financial account list, we should pass customerId here and filter by it.

        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        if (financialAccountFilter.AccountType != null)
        {
            AddCriteria(x => x.AccountType == financialAccountFilter.AccountType);
        }

        AddCriteria(x => x.IsSystemAccount == financialAccountFilter.IsSystemAccount);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x =>
                // Common fields
                (x.BrokerName != null && x.BrokerName.Contains(filter.Search)) ||
                (x.HolderName != null && x.HolderName.Contains(filter.Search)) ||

                // Local Bank Account fields
                (x.LocalAccount != null && (
                    (x.LocalAccount.ShabaNumber != null && x.LocalAccount.ShabaNumber.Contains(filter.Search)) ||
                    (x.LocalAccount.CardNumber != null && x.LocalAccount.CardNumber.Contains(filter.Search)) ||
                    x.LocalAccount.AccountNumber.Contains(filter.Search)
                )) ||

                // International Bank Account fields
                (x.InternationalAccount != null && (
                    (x.InternationalAccount.IbanNumber != null && x.InternationalAccount.IbanNumber.Contains(filter.Search)) ||
                    (x.InternationalAccount.SwiftBicCode != null && x.InternationalAccount.SwiftBicCode.Contains(filter.Search)) ||
                    x.InternationalAccount.AccountNumber.Contains(filter.Search)
                )) ||

                // Cash Account fields
                (x.CashAccount != null &&
                    x.CashAccount.Title != null && x.CashAccount.Title.Contains(filter.Search)) ||

                // Price Unit fields
                (x.PriceUnit != null &&
                    x.PriceUnit.Title.Contains(filter.Search))
            );
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(FinancialAccount.CreatedAt), SortDirection.Descending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}