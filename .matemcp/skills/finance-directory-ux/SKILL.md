
# GoldEx Finance Directories — Payment Vouchers & Checks

## Scope
- `/finances/payment-vouchers`: `Pages/Finances/PaymentVouchers/Components/PaymentVouchersList.razor[.cs|.css]`.
- `/finances/checks`: `Pages/Finances/Checks/Components/CheckPaymentsList.razor[.cs|.css]`.
- Pages use the enterprise shared AppBar breadcrumbs. No page-local MudBreadcrumbs or old MudTable on these route lists.

## Responsive directories
- Use semantic desktop tables and compact mobile cards, showing the most useful fields first. No decorative arrows. Standard font weights: 400 text, 500 titles, 600 single most important numeric amount. Dark/light colors from palette vars, stable 44px filter buttons, clear loading/failure/empty states, 10/25/50 pagination and effective server filtering.
- PaymentVouchersList preserves `IPaymentVoucherService.GetListAsync(RequestFilter, PaymentVoucherFilter, CustomerId)`, payment type and date filters and sorting fields, invoice-associated documents' view-only actions, and direct documents' edit/delete actions. `?create=true` continues to open Editor.
- CheckPaymentsList preserves `ICheckPaymentService.GetListAsync(RequestFilter, CheckPaymentFilter)`, status/due date filters, overdue pending styling, read/accept/return for pending only, and the existing financial settlement flows.
- Both lists respect q query search and preserve other query dimensions. CSS isolation requires `::deep` for MudSelect filter/sort classes and ShellIcon SVG descendants; hide sort and direction actions on mobile to avoid stray icons/row gaps.

## Dialogs and destructive operations
- Payment Editor uses `EditorDialogFrame` and `ResponsiveDialogService`, keeping original validation, accounting service actions, unit/exchange-rate, customer and bank accounts. Nested customer and account editors use native responsive dialogs to avoid stacking MudDialog behind the native top layer.
- Checks' filter, read, accept and return dialogs use EditorDialogFrame/ResponsiveDialogSession and preserve AcceptCheckPaymentRequest, ReturnCheckPaymentRequest, FX adjustments and destination-account selection. Never simplify these financial business rules in UI work.
- Customer, invoice and inventory product `Remove.razor` also use the same centered native confirmation styling, but each component continues calling its ORIGINAL service and backend validations. Do not replace their service calls with a generic delete endpoint.
- The global confirmation CSS is `.gex-delete-confirm` in `appearance.css`.

## Verification
- Targeted `node --test scripts/tests/finance-directories-refactor.test.mjs scripts/tests/customer-transfer-directory.test.mjs`.
- `dotnet build src/App/Client/GoldEx.Client/GoldEx.Client.csproj --no-restore -v:q /m:1`.
- Visual validation of authenticated rendered app needed before production; a compiled test does not validate actual mobile dimensions or native top-layer behavior.
- Enterprise release 1.0.122 contains consolidated unshipped UI changes. Local Dev commit only unless the user requests push/deployment.
