import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const root='src/App/Client/GoldEx.Client/Pages/';
const route=read(root+'InventoryStocks/List.razor');
const legacy=read(root+'InventoryStocks/Components/InventoryStockList.razor');
const directory=read(root+'InventoryStocks/Components/InventoryDirectory.razor');
const code=read(root+'InventoryStocks/Components/InventoryDirectory.razor.cs');
const styles=read(root+'InventoryStocks/Components/InventoryDirectory.razor.css');
const dialog=read(root+'InventoryStocks/Components/InventoryDirectoryFiltersDialog.razor');
const dialogCode=read(root+'InventoryStocks/Components/InventoryDirectoryFiltersDialog.razor.cs');
const trace=read(root+'InventoryStocks/Components/InventoryTraceTimeline.razor');
const traceCode=read(root+'InventoryStocks/Components/InventoryTraceTimeline.razor.cs');
const traceStyles=read(root+'InventoryStocks/Components/InventoryTraceTimeline.razor.css');
const invoicesCss=read(root+'Invoices/Components/InvoicesList.razor.css');
const js=read('src/App/Server/GoldEx.Server/wwwroot/js/invoice-sticky-header.js');

test('/inventory-stocks/list uses new custom directory without changing reusable selection MudTable',()=>{
 assert.match(route, /Route\(ClientRoutes.InventoryStocks.List\)/);
 assert.match(route, /<InventoryDirectory SearchQuery="@SearchQuery" PersistState="true" \/>/);
 assert.doesNotMatch(route, /<InventoryStockList/);
 assert.doesNotMatch(directory, /<MudTable|<MudPagination|<MudTableSortLabel/);
 assert.match(legacy, /<MudTable T="InventoryStockVm"/);
 assert.match(legacy, /MultiSelection="@Selectable"/);
 assert.match(directory, /class="stock-desktop-table"/);
 assert.match(directory, /class="stock-mobile-list"/);
 assert.match(styles, /@media \(max-width: 959\.98px\)/);
 assert.match(styles, /\.stock-desktop-table \{ display: none; \}/);
});

test('inventory toolbar mirrors invoices with filters, sort, server pagination and skeletons',()=>{
 assert.match(directory,/class="directory-heading"/);
 assert.match(directory,/class="directory-toolbar"/);
 assert.match(directory,/Label="جستجوی انبار"/);
 assert.match(directory,/DebounceInterval="500"/);
 assert.match(directory,/OnClick="OpenMobileFilters"/);
 assert.match(directory,/Class="directory-sort-direction"/);
 assert.match(directory,/class="stock-filter-chips"/);
 assert.match(directory,/class="stock-skeleton-table"/);
 assert.match(directory,/class="stock-skeleton-card"/);
 assert.match(directory,/class="stock-pagination"/);
 assert.match(directory,/value="@_pageSize" @onchange="ChangePageSize"/);
 for(const size of [10,25,50]) assert.match(directory,new RegExp('<option value="'+size+'">'));
 assert.match(directory,/@onclick="\(\) => ChangePage\(-1\)"/);
 assert.match(directory,/@onclick="\(\) => ChangePage\(1\)"/);
 assert.match(code,/new RequestFilter\(_pageIndex \* _pageSize, _pageSize, SearchQuery/);
 assert.match(code,/cancelPrevious: true/);
 assert.match(code,/onFailure: \(\) => _loadFailed = true/);
 assert.match(code,/await InvokeAsync\(StateHasChanged\);\s*await Task.Yield\(\);/);
 assert.match(code,/SizeQuery is 10 or 25 or 50/);
 assert.match(code,/if \(changed\) await LoadDirectoryAsync\(\)/);
});

test('sticky inventory table header tracks the toolbar height with the reused tracker',()=>{
 assert.match(code,/trackFilterHeight", _directory, _filterBar/);
 assert.match(styles,/--invoice-filters-block-size, 96px/);
 assert.match(js,/--invoice-filters-block-size/);
 assert.match(styles,/\.stock-table th \{\s*position: sticky;/);
 assert.match(styles,/\.directory-filters \{\s*position: sticky;/);
 assert.match(code,/await _stickyTracker.InvokeVoidAsync\("dispose"\)/);
});

test('inventory filters honor type/status/category/date and maintain responsive native dialog',()=>{
 assert.match(directory,/<InventoryTraceTimeline ItemId=/);
 assert.match(code,/new InventoryFilter\(_actionType, ItemType, _categoryFilter\?\.Id/);
 assert.match(code,/new Dictionary<string, object>/);
 assert.match(code,/ResponsiveDialogs.Show<InventoryDirectoryFiltersDialog>/);
 assert.match(code,/InventoryDirectoryFiltersDialog.SortLabel/);
 assert.match(code,/ItemType is not ItemType.UsedProduct && result.ItemStatus is ItemStatus.Melted/);
 assert.match(dialog,/<EditorDialogFrame>/);
 assert.doesNotMatch(dialog,/<MudDialog>/);
 assert.match(dialog,/<MudDateRangePicker/);
 assert.match(dialog,/Class="gex-filter-mobile-sort"/);
 assert.match(dialogCode,/Session\?\.Close\(new InventoryDirectory.MobileFiltersResult/);
 assert.match(code,/parameters.Add\("size", _pageSize\)/);
 assert.match(code,/parameters.Add\("page", pageIndex \+ 1\)/);
 assert.match(code,/parameters.Add\("category", _categoryFilter.Id\)/);
});

test('all legacy inventory operations remain and now have accessible icon actions',()=>{
 for(const handler of ['OnOpenVitrineDialog','OnCopyProductVitrineUrl','OnPrintProductBarcode',
     'OnPrintCoinBarcode','OnEditProduct','OnRemove']){
   assert.ok(directory.includes('@onclick="() => '+handler+'(item)"'),handler+' missing in markup');
   assert.ok(code.includes('Task '+handler+'(InventoryStockVm '),handler+' missing implementation');
 }
 assert.match(directory,/item.SoldAmount == 0/);
 assert.match(directory,/class="stock-expand" aria-expanded=/);
 assert.match(directory,/class="stock-card-expand" aria-expanded=/);
 assert.match(code,/private void ToggleDetails\(InventoryStockVm item\)/);
 assert.match(directory,/ItemType is ItemType.MoltenGold/);
 assert.match(directory,/تاریخ ری‌گیری/);
 assert.match(directory,/GetCoinMintYear\(item.Coin\)/);
 assert.match(directory,/FormattedAmount\(item.CurrentAmount, item\)/);
 assert.match(directory,/FormattedAmount\(item.SoldAmount, item\)/);
});

test('history details are not MudTable: paged semantic entry/exit timeline',()=>{
 assert.doesNotMatch(trace, /MudTable|MudPagination/);
 assert.match(trace,/<ol class="stock-history-list">/);
 assert.match(trace, /row.ActionType == WarehouseActionType.In/);
 assert.match(trace, /row.Description/);
 assert.match(trace, /row.SourceUrl/);
 assert.match(trace, /ChangePage\(1\)/);
 assert.match(traceCode, /GetInventoryStockTracesAsync\(ItemId, ItemType/);
 assert.match(traceCode,/private const int _pageSize = 6/);
 assert.match(traceStyles,/\.stock-history-entry:not\(:last-child\)::before/);
 assert.match(styles,/\.stock-card\.is-empty/);
 assert.match(styles,/\.stock-card\.is-partial/);
});

test('new invoice pill matches new customer; new inventory entry is a single route action',()=>{
 assert.match(invoicesCss,/\.directory-heading ::deep \.directory-create \.mud-button-root,[\s\S]*?border-radius: 999px/);
 assert.match(directory,/Href="@ClientRoutes.InventoryStocks.InventoryEntry.Index"/);
 assert.match(directory,/Class="directory-create"/);
 assert.match(styles,/\.stock-directory \.directory-heading ::deep \.directory-create \{ border-radius: 999px; \}/);
});

test('barcode/QR deep links still infer item type and availability, like the legacy list',()=>{
 assert.match(code,/Guid.TryParse\(SearchQuery, out _\)/);
 assert.match(code,/MatchSearchedItem\(matched\)/);
 assert.match(code,/if \(item.Coin is not null\) ItemType = ItemType.Coin/);
 assert.match(code,/else if \(item.Currency is not null\) ItemType = ItemType.Currency/);
 assert.match(code,/ItemStatus = item.CurrentAmount <= 0 \? ItemStatus.Sold : ItemStatus.Available/);
});