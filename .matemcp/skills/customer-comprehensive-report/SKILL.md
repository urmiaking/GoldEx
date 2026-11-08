# Customer Comprehensive Report - GoldEx

## Purpose
Full customer dossier with invoice sources, line details, payments, and official ledger plus standard print output.

## Invariants
- Do not create multiple invoice PDFs or modify invoice posting.
- Authoritative balances must derive only from ITransactionRepository.GetCustomerTransactionsAsync, and documentary invoice/payment amounts must not be added again.
- Date-filtered reports fetch ledger through ToDate from the beginning; filter visible rows after calculating running balances.
- Group totals by transaction direction and price unit (gold weight vs cash remain separate).
- Respect tenant global filters and admin/owner authorization.
- Share CustomerComprehensiveDocument between web and printable pages, using the existing ReportPrintBase.
- Verify with node tests and targeted server build. Keep incomplete local Dev work untouched.
