# GoldEx Melting Batches UX — Inventory to Molten Gold

## Route scope
- Index: `/inventory-stocks/melting-batches` (constant `ClientRoutes.InventoryStocks.MeltingBatches.Index`); `Pages/InventoryStocks/MeltingBatches.razor`.
- Create/edit wizard: `Pages/InventoryStocks/SetMeltingBatch.razor`, hosting `Components/MeltingBatchForm.razor[.cs|.css]`.
- The Index and Create routes use the global AppBar breadcrumb, not a second page-local MudBreadcrumbs.

## Directory
- `Components/MeltingBatchesList.razor[.cs|.css]` replaces the old MudTable with a native desktop table and compact independent mobile cards. Both expose batch number, total weight, date, status, laboratory, expand-details and the stage-specific action: Melting -> Send to Lab; SentToLab -> Complete with final molten product; Completed -> Barcode/Details.
- `MeltingBatchDetails.razor[.css]` renders original product list with barcode, type, weight and fineness, completed molten product with assay number and print-barcode, waste weight/percent and linked ledger transactions; this is expandable to keep the main page compact. Do not remove any of those financial/audit details.
- `MeltingBatchFiltersDialog.razor[.cs]`: `ResponsiveDialogService`/`EditorDialogFrame`; status, date range with scoped `gex-filter-date-range` RTL calendar adornment, mobile-only sort label and ascending/descending. Server request is `IMeltingBatchService.GetListAsync(RequestFilter, MeltingBatchFilter)`; filter must be applied before paging, use 10 default rows (25/50 optional), cancel old requests and handle loading/retry/empty.
- Keep the original JS label print dependencies (`jsbarcode.all.min.js`, `print.js`) and `printBarcode` entrypoint. Sticky filter uses `invoice-sticky-header.js` and `--invoice-filters-block-size`, disposed on component disposal.
- Weight values and assay details are valuable; standard font weights 400/500 and 600 only for the most important measurements. Avoid ornamental arrows or oversized mobile cards.

## Wizard
- `MeltingBatchForm.razor` keeps the existing 3-step MudStepper and event flow, in this exact order: choose available UsedProduct items -> send to AssayingLab -> provide molten gold weight/fineness/assay number/fees/accounts and complete.
- Preserve `IMeltingBatchService.CreateAsync(_model.ToMeltingRequest())`, `SendToLabAsync(_id.Value, _model.ToSendToLabRequest())`, `CompleteMeltingAsync(_id.Value, _model.ToCompleteMeltingRequest())` and their existing success/failure state handling; no domain formula/backend change. The embedded selectable `InventoryStockList` must NOT be refactored globally: it is shared with other workflows.
- `MeltingBatchConfirmDialog` is a native responsive confirmation for all 3 mutations. Only affirmative results may invoke the underlying service. `OnAddFinancialAccount` and `LabSubmissionStep.OnAddAssayer` use ResponsiveDialogs to prevent an inner MudDialog behind the native overlay.
- Step visual styles are scoped through `MeltingBatchForm.razor.css` using `::deep` for children. Preserve the processing disabled states, stage error/success notifications, monetary/weight calculations and account selectors. Mobile should stack lab inputs and show compact metric chips.
- The lab, item and completion step components are still under `Components/MeltingBatches/`.

## Validation / delivery
- Static regression test: `node --test scripts/tests/melting-batches-redesign.test.mjs`.
- Targeted client compilation: `dotnet build src/App/Client/GoldEx.Client/GoldEx.Client.csproj --no-restore -v:q /m:1`.
- Explicit authenticated browser verification of mobile layout and step transitions is desirable before deployment; build/tests cannot guarantee rendered appearance.
- Current unshipped GoldEx release notes are consolidated in version 1.0.122. Local Dev commits only unless user authorizes push and deployment.
