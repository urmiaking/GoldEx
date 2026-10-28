# Invoice List UX — GoldEx

Use when modifying the enterprise invoice list, child transaction timeline, inventory movement timeline, or pagination.

## Relevant files

- `src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor` and its `.razor.cs`/`.razor.css`
- `src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor` and its `.razor.css`
- `src/App/Server/GoldEx.Server/releases.json`
- `scripts/tests/invoice-directory.test.mjs` and `scripts/tests/invoice-paging-timeline.test.mjs`

Read `AGENTS.md` (especially the sections on invoice directory UI, color/typography and release notes) before editing; it is the source of truth.

## Invariants

1. Keep a semantic native table on desktop and cards on mobile, without MudTable or MudPagination. Preserve server-side filtering, sorting and the existing invoice API/commands.
2. The initial load and every page change should show responsive skeletons; yield to Blazor rendering after setting the loading flag and before starting the service request. Keep loading/error/empty states distinct.
3. Pagination choices: 10 by default, 25, 50. In RTL, size selector belongs on the right, previous/page/next on the left. Normalize old query `size=14` to 10.
4. Paid invoices have a full green success chip in both views. Background `--mud-palette-success` and foreground `--mud-palette-success-text` respect each theme's correct contrast.
5. Expanded financial details use a timeline rather than a nested table. `TransactionType.Credit` = green بستانکار, `TransactionType.Debit` = red بدهکار; do not infer polarity from the numeric amount. Stock `WarehouseActionType.In` is ورود به انبار, `Out` خروج از انبار.
6. Preserve details' independent server pagination, affected account, amount/weight, descriptions, dates and customer current/historical balances. Keep lazy visibility activation so desktop and mobile don't duplicate requests. Use unique tab/panel IDs.
7. Validate with `node --test scripts/tests/*.test.mjs`, `git diff --check`. Follow the repository's UI-only build policy and update `releases.json` with plain Persian release notes.

## Mobile visual hierarchy and balance-state accents (#249)

- Use the existing `InvoicesList.UnpaidClass(invoice)` for a mobile-only 5%-opacity status glow and a thin RTL start border. `is-credit` is green, `is-debit` / `is-overdue` red, `is-paid`/neutral remain plain. Do not color the entire customer card based on a single rotating multi-currency remaining amount.
- Mobile invoice header chips have at least 26px visual height and consistent 500-weight font. The paid success chip in the amounts area is smaller (28px) so that it doesn't dominate the financial values.
- `appearance.css` uses weight 500 for the sidebar 'فاکتور جدید', active desktop menu link, selected bottom mobile link and bottom create link. Active state is indicated by background and icon contrast.
- Desktop invoice create menu aligns with the outer filter card by a desktop-only -16px inline-end margin, never on mobile.
- Validate via `scripts/tests/invoice-mobile-tone.test.mjs` and complete Node UI tests.