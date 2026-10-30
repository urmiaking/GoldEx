# GoldEx Enterprise Reporting UX

## Report page routing
- `Pages/Reporting/Index.razor` and the 14 `Pages/Reporting/Pages/Reports/*.razor` use the shared AppBar breadcrumb; do not reintroduce page-local `MudBreadcrumbs`. Their `MudContainer` carries `gex-report-page` for scoped legacy-report styling in `appearance.css`.
- Legacy report tables (13 report views) retain existing report DTOs, sorting, actions and printing. They have neutral `Elevation=0` and common lighter typography/spacing. This is transitional; their full per-report mobile card migrations are not yet complete. Never confuse a cosmetic table restyle with a replacement.

## Shared report filter
- `Components/ReportFiltersBase.razor[.css]` owns the header/filter grid, 1 date range picker (RTL leading calendar icon, `gex-filter-date-range`), responsive desktop/mobile apply action and existing `EditForm`/DataAnnotations validation. Report-specific `Filters/*.razor` supply their own MudItem fields via `ChildContent`.
- `OnSubmit.InvokeAsync(Model)` must occur only on valid submit. Maintain original filter DTO and API business query; date start/end and print URL semantics remain unchanged.
- CSS typography defaults 400 for prose, 500 headings/controls, no heavy shadows.

## Customer balance report first full migration
- `Components/Views/CustomerBalanceReportView.razor[.cs|.css]` has a semantic desktop table and compact mobile customer cards with Blazor Virtualize (no paginator), name/payable/receivable/net sorting, source navigation, one shared print action.
- Its `CustomerBalanceReportSummary` still groups **all Items** by price unit; never base ledger totals on rendered virtual rows. Existing positive/negative sign formatting, names and ledger navigation are preserved.
- `Items` may be replaced as filters change: rebuild the sorted virtualized item list then. `@key=row` avoids duplicate render keys if a customer appears with different currencies.
- Print route/components `ReportPrintBase`, `ReportBase` and all print markup were untouched.

## Validation & delivery
- Targeted `node --test scripts/tests/reporting-modernization.test.mjs`, client build on structural changes, `git diff --check`.
- Authenticated visual verification of desktop/mobile filters and report cards still needed before production.
- Notes appended to unshipped GoldEx enterprise v1.0.122; commit locally on Dev without push/deploy unless authorized.
## User screenshot regression: duplicate report action and profit-analysis UX
- The previous report-filter layout had TWO buttons with ButtonType.Submit in separate desktop/mobile areas, and CSS isolation on generated MudItem markup did not hide one reliably. Keep EXACTLY ONE submit button in a native `div.report-filter-actions` after the shared MudGrid, with desktop end-alignment and mobile width:100%. Do not add a second breakpoint-dependent action. Date range and specialized fields remain editable in one EditForm.
- `ProfitAnalysisReportView.razor[.cs|.css]` now uses a native desktop semantic table and compact mobile cards, exactly one print control, per-row invoice navigation, expanded details/purchase reference and Blazor Virtualize with no client pagination. Preserve the original `ProfitAnalysisSummary Report="Report"` from the full report DTO, not just visible virtualized items, and maintain `ShowExchangeRate` conditional rows. Rendering/printing do not change API calculations.
- Do not use old thick-bordered, striped, bold MudTable here. RTL and dark-mode colors reference palette variables. Test with `scripts/tests/ux-regressions-oct30.test.mjs`. Authenticated desktop/mobile screenshots still require visual review.
## 2026-10-30 — reporting layout, virtualization, skeleton follow-up
- User rejected the detached full-width submit row and excessive gaps. ReportFiltersBase must render exactly ONE Submit button as the last MudItem INSIDE the existing MudGrid. Native div.report-filter-body is necessary for CSS-isolated ::deep .mud-grid and .mud-grid-item selectors to apply to MudBlazor-generated HTML. Desktop fields flex-grow with 10px gap, action is inline; mobile stacks.
- User rejected 10/25/50 pagination in reporting. Both custom report views use native Blazor Virtualize inside desktop tbody (SpacerElement=tr), plus mobile cards Virtualize. Desktop scroll is bounded with a sticky header. Keep sorting and drilldown intact. Full report summaries always use all source records.
- ALL 14 report view Loading branches render reusable ReportTableSkeleton rather than MudProgressLinear. The skeleton has desktop row shapes and mobile card shapes. Do not change report printing or PDF templates.
- Regressions: scripts/tests/report-virtualization-skeleton.test.mjs, reporting-modernization.test.mjs, ux-regressions-oct30.test.mjs. Authenticated browser visual QA still pending. Version 1.0.122 remains unshipped.
