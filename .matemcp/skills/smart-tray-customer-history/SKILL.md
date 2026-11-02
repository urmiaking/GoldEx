# Smart tray and customer history UI
- SmartTrayPage creates trays directly, guards duplicate clicks, and opens the created tray. The server generates the default title using the allocated tray number when Title is empty; older clients can still supply custom names.
- Preserve scan, item movements, discrepancy handling, invoices, hubs and audit semantics when modifying tray UI.
- Customer card actions open CustomerHistoryDialog with ResponsiveDialogs.Show(wide:true). It hosts TransactionList or InvoicesList for the same CustomerId inside EditorDialogFrame and keeps their pagination and search. Modal uses native desktop and mobile variants.
- Use 400/500 weights and rounded surfaces, ::deep for Mud components. Test smart-tray-customer-dialog-ux.test.mjs, build for structural changes, commit locally only.
