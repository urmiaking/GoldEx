# Smart tray and customer history UI
- SmartTrayPage creates trays directly, guards duplicate clicks, and opens the created tray. The server generates the default title using the allocated tray number when Title is empty; older clients can still supply custom names.
- Preserve scan, item movements, discrepancy handling, invoices, hubs and audit semantics when modifying tray UI.
- Customer card actions open CustomerHistoryDialog with ResponsiveDialogs.Show(wide:true). It hosts TransactionList or InvoicesList for the same CustomerId inside EditorDialogFrame and keeps their pagination and search. Modal uses native desktop and mobile variants.
- Use 400/500 weights and rounded surfaces, ::deep for Mud components. Test smart-tray-customer-dialog-ux.test.mjs, build for structural changes, commit locally only.

- CustomerHistoryDialog renders CustomerLedgerDirectory (native desktop table/mobile cards) or InvoicesList (dialog cards) in a bounded flex modal. Do not nest EditorDialogFrame for this read-only list. Do not attach the invoice page sticky observer to CustomerId-specific views.

- Customer invoices must remain original InvoicesList table in desktop modal >=960px, original cards below 960px, preserving invoice-paid foreground contrast. CustomerLedgerDirectory is now an unpaginated accounting timeline; retrieve filtered customer transactions in 100-item batches following InvoiceDetails, not a desktop table or mobile card grid.
- Desktop customer invoices opt into extraWide:true (1280px max) in CustomersList.OnViewInvoices; other dialogs retain existing sizing. Mobile sheet and invoice cards are unchanged.

- Closing customer invoice history must not call CustomersList.RefreshAsync (view-only); other customer mutations still refresh. Desktop shared modal uses 20px radius with overflow:hidden so footer corners are rounded. Mobile retains its own radius.
