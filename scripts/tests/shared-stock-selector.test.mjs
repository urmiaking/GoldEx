import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/', import.meta.url);
const read = path => readFileSync(new URL(path,root),'utf8');

test('shared inventory list routes only selectable mode to responsive selection component',()=>{
 const view=read('InventoryStocks/Components/InventoryStockList.razor');
 const logic=read('InventoryStocks/Components/InventoryStockList.razor.cs');
 assert.match(view, /@if \(Selectable\)/);
 assert.match(view, /<InventoryStockSelection/);
 assert.match(view, /SelectedItemsChanged="SelectedItemsChanged"/);
 assert.match(view, /else\s*\{\s*<MudTable/);
 assert.match(logic,/if \(Selectable\) return;/);
});
test('selection retains server-side paging, filters and multi-selection across pages',()=>{
 const view=read('InventoryStocks/Components/InventoryStockSelection.razor');
 const logic=read('InventoryStocks/Components/InventoryStockSelection.razor.cs');
 assert.match(view,/stock-selector-desktop/);
 assert.match(view,/stock-selector-mobile/);
 assert.match(view,/MudCheckBox T="bool"/);
 assert.match(view,/aria-label="صفحه‌بندی انتخاب کالا"/);
 assert.doesNotMatch(view,/<MudTable/);
 assert.match(logic,/Dictionary<string, InventoryStockVm> _selected/);
 assert.match(logic,/new RequestFilter\(_pageIndex \* _pageSize, _pageSize/);
 assert.match(logic,/new InventoryFilter\(/);
 assert.match(logic,/GetListAsync\(request, inventory, ct\)/);
 assert.match(logic,/cancelPrevious: true/);
 assert.match(logic,/new HashSet<InventoryStockVm>\(_selected.Values\)/);
 assert.match(logic,/private int _pageIndex, _pageSize = 10/);
 assert.match(logic,/await ClearSelection\(\);\s*await RefreshAsync\(\);/);
});
test('invoice consumer handles cleared or changed item type without stale result model',()=>{
 const code=read('Invoices/Components/InventoryItemSelector.razor.cs');
 assert.match(code,/_products = null;\s*_coins = null;\s*_currencies = null;/);
 assert.match(code,/if \(items.Count == 0\) return;/);
 for(const key of ['Task.WhenAll(productItemTasks)','Task.WhenAll(coinItemTasks)','Task.WhenAll(currencyItemTasks)']) assert.ok(code.includes(key));
});
test('all three consumer workflows stay wired and kardex clears stale selection',()=>{
 for(const file of ['Invoices/Components/InventoryItemSelector.razor',
 'InventoryStocks/Components/MeltingBatches/ProductSelectionStep.razor',
 'Reporting/Components/InventoryKardexSelector.razor']){
 const view=read(file);
 assert.match(view,/<InventoryStockList/);
 assert.match(view,/Selectable="true"/);
 assert.match(view,/SelectedItemsChanged=/);
 }
 assert.match(read('Reporting/Components/InventoryKardexSelector.razor.cs'),/_selectedItems = items\?\.ToList\(\) \?\? \[\]/);
 assert.match(read('InventoryStocks/Components/MeltingBatches/ProductSelectionStep.razor.cs'),/Model.Products = inventoryItems\?/);
});
