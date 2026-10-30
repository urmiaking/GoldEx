# GoldEx Customer Transfer Voucher Directory UX

Scope: enterprise route `/finances/customer-transfers`. The route component `Pages/Finances/CustomerTransfers/List.razor` delegates to `Components/CustomerTransferList.razor[.cs|.css]`. Do not reintroduce page-local breadcrumbs or the old MudTable; the shared shell owns breadcrumb navigation.

## Responsive directory
- Desktop: sticky search/filter/sort toolbar + sticky native semantic table aligned with Customers, Invoices and Inventory directory; mobile (<960px): independent cards with clear Source -> Destination names, amount/unit, optional source/destination invoice references, description and edit/delete buttons.
- Use server-side GetListAsync(RequestFilter, CustomerTransferVoucherFilter) for search, sort (TransferDate/VoucherNumber/Amount), date range, price unit, customer scoping, 10/25/50 paging and total. Never filter after paging or change the finance accounting service.
- Display skeletons while loading, explicit failure retry, empty-state reset. `invoice-sticky-header.js` observer must be disposed with `base.DisposeAsync()`. Theme palette variables, RTL and reduced-motion support remain mandatory.
- Keep route `?create=true` handoff into the list OnCreate method and `?q=` search behavior. Other filters are dialog-local (not serialized to URL).

## Responsive dialogs
- `CustomerTransferFiltersDialog` via ResponsiveDialogService/EditorDialogFrame: date picker uses scoped `gex-filter-date-range` RTL adornment, nullable price-unit selection, sort options visible on mobile, Apply/Reset/Cancel.
- `CustomerTransferEditor` uses EditorDialogFrame for create/edit, with responsive cash/gold balance preview, sender/receiver customer autocomplete, unit/amount/exchange rate, invoice selectors, description and old validation/accounting semantics intact. The nested new-customer dialog MUST also use ResponsiveDialogs.Show<Customers.Components.Editor> with ReturnModel=true and ShowFinancialAccounts=false (native top-layer nesting).
- `CustomerTransferDeleteDialog` is a native responsive confirmation; only the affirmative response triggers existing DeleteAsync and list refresh.
- Keep the mobile navigation and Desktop Dock unchanged.

## Verification
- Run targeted `node --test scripts/tests/customer-transfer-directory.test.mjs` and `dotnet build src/App/Client/GoldEx.Client/GoldEx.Client.csproj --no-restore -v:q /m:1` for this structural route migration. Do not run heavy screenshot tests unless requested.
- Existing user-facing release 1.0.122 gathers these unshipped UX changes. Local Dev commits only unless push/deployment requested.

## Mobile compact correction (2026-10-30)
- Screenshot feedback supersedes old vertical mobile card with red/green arrows. A compact card must use a header for voucher number/date and amount, a single from/to row with plain text (no direction icons), optional invoice metadata, then small edit/delete buttons.
- Style text at regular weight 400/500. Only the amount may use 600. On screens <960px, hide both child-component MudSelect sort and direction icon through CSS-isolated `::deep` selectors; otherwise the toolbar can show a stray arrow or empty second row.
- Regression in `scripts/tests/finance-directories-refactor.test.mjs`. No heavy screenshot CI.
