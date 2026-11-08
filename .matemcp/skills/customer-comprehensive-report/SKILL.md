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

## Compact report design (#280)
- Use one compact overview with per-unit sales, purchases and final ledger balances; do not display technical row or document counters.
- Use one grouped invoice table with header rows and item rows inside separate tbody groups; all invoice items must remain printable with repeated column headers.
- Use one chronological ledger/payments table without double-counting posted payments. Show unmatched payment documents as documentary rows without fabricating accounting debit/credit.
- Nullable Razor customer names require @(_customer?.FullName) explicitly in text/parameter attributes.
- The view has an explicit portrait/landscape selector, forwarded as landscape query parameter into ReportPrintBase. In print, suppress its redundant detail-heading and notes without changing defaults used by other reports.

## Correct printing and summary item count (#282)
- Bind the HTML orientation selector to a string with stable landscape / portrait option values, never to a bool with lower-case HTML values. Send landscape = _printOrientation == "landscape" to the existing bool print query parameter.
- IncludeDetails=false deliberately returns an empty Items list. Never display Items.Count as the number of invoice items; the API returns ItemCount computed from ProductItems+UsedProducts+CoinItems+CurrencyItems before materializing optional details.
- Do not merge this fix until the user verifies it against their local database in the browser.
