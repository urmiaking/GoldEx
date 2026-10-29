# GoldEx Inventory Directory UI

Use for the enterprise `/inventory-stocks/list` route and responsive design.

## Component boundaries

- `src/App/Client/GoldEx.Client/Pages/InventoryStocks/List.razor` uses the route-only `InventoryDirectory.razor[.cs|.css]`, replacing MudTable only on the main list page.
- **Keep `InventoryStockList.razor` unchanged.** Its MudTable is reused by item-selection and stock workflows, including multi-selection. Refactoring that component would break important existing consumers.
- `InventoryDirectoryFiltersDialog` uses `ResponsiveDialogService` and shared native `EditorDialogFrame`. Sort is mobile-only inside the dialog since desktop sorting is in the toolbar. Product category appears only for Product; UsedProduct supports Melted rather than Sold. Date range uses the plain outlined MudDateRangePicker, no padded wrapper.
- `InventoryTraceTimeline` is custom non-Mud paged movement history, with its own isolated CSS; child-component elements do not receive a parent's Razor CSS-isolation attribute. Date, amount, In/Out action, description, source link, and 6-row Previous/Next pages are visible.
- Preserve actions and domain behavior: product/used/coin/currency/molten presentations with correct weight or count units; Vitrine management/link, label printing, product edit and delete if SoldAmount==0, and original IInventoryStockService.GetListAsync with InventoryFilter. Keep selected store behavior, no change to accounting or stock quantities.

## Interaction details

- Invoice-style directory heading, sticky filter/search/sort toolbar, desktop sticky semantic table, mobile touch cards, restrained green/red inventory status borders, responsive skeletons including pagination loads, retry/empty states, and 10 default rows with 25/50 options.
- URL persistence covers page/size/q/type/status/category/start/end/sort/asc; compare effective query state on OnParametersSetAsync so a replace-navigation after a successful API call does not issue a duplicate request. Reset defaults correctly when parameters disappear via browser Back.
- Loading sets StateHasChanged and yields before the async IInventoryStockService request; cancelPrevious: true. Use existing invoice-sticky-header.js trackFilterHeight; it sets the CSS property `--invoice-filters-block-size` even for inventory, so the inventory header CSS must reuse that exact variable. Dispose its JS observer/module.
- Use `var(--mud-palette-*)` for dark/light themes. Preserve RTL, keyboard accessible labels, reduced-motion and a compact pager. The new invoice dropdown's rounded edge is scoped in InvoicesList.razor.css.
- Static regression: `scripts/tests/inventory-directory-ux.test.mjs`, full Node test suite. Full GoldEx.Client build is appropriate for a route/component refactor. Issue #267, version 1.0.115; local Dev commit only unless asked to push.

- Preserve GUID barcode/QR deep links: when a query resolves to an item despite the initial type filter, infer Product/Coin/Currency from the response item, set Available/Sold according to CurrentAmount, and keep WarehouseActionType consistent. This mirrors the legacy InventoryStockList special search behavior.