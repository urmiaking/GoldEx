import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read=p=>readFileSync(new URL('../../src/App/Client/GoldEx.Client/Pages/'+p,import.meta.url),'utf8');
const stock=read('InventoryStocks/Components/InventoryDirectory.razor');
const stockCss=read('InventoryStocks/Components/InventoryDirectory.razor.css');
const history=read('InventoryStocks/Components/InventoryTraceTimeline.razor');
const historyCss=read('InventoryStocks/Components/InventoryTraceTimeline.razor.css');
const invoice=read('Invoices/Components/InvoicesList.razor');
const invoiceCss=read('Invoices/Components/InvoicesList.razor.css');
const customer=read('Customers/Components/CustomersList.razor');
const customerCard=read('Customers/Components/CustomerCard.razor');
const customerCss=read('Customers/Components/CustomerCard.razor.css');

test('no applied-filter strips; existing count badges remain in all three directories',()=>{
 for(const markup of [stock,invoice,customer]){
  assert.doesNotMatch(markup,/directory-applied-filters|stock-filter-chips|stock-clear-filters/);
  assert.match(markup,/directory-filter-count/);
  assert.match(markup,/OpenMobileFilters/);
 }
});
test('inventory entry action aligns with customer new action, not negative left offset',()=>{
 assert.match(stock,/Href="@ClientRoutes.InventoryStocks.InventoryEntry.Index"/);
 assert.match(stock,/Class="directory-create"/);
 assert.match(stockCss,/\.stock-directory \.directory-heading ::deep \.directory-create \{ margin-inline:0; \}/);
 assert.doesNotMatch(stockCss,/margin-inline-end:\s*-16px/);
});
test('compact stock movement lists action, amount, date, source action on one flex row',()=>{
 assert.match(history,/class="stock-history-line"/);
 assert.match(history,/row.Amount.ToWeightFormat/);
 assert.match(history,/row.DateTime.ToString/);
 assert.match(history,/class="stock-history-source"/);
 assert.match(history,/aria-label="@GetSourceText\(row.SourceUrl\)"/);
 assert.match(history,/row.Description/);
 assert.match(historyCss,/\.stock-history-line \{ display:flex; align-items:center; flex-wrap:wrap/);
 assert.match(historyCss,/min-height:28px/);
});
test('mobile inventory fineness and barcode now share inline metadata; two compact amount panels',()=>{
 assert.match(stock,/class="stock-card-identifiers"/);
 assert.match(stock,/عیار @item.Product.Fineness.ToString\("G29"\)/);
 assert.match(stockCss,/\.stock-card-identifiers \{ display:flex; align-items:center; flex-wrap:wrap/);
 assert.match(stockCss,/\.stock-card-amounts > div:first-child/);
 assert.match(stockCss,/grid-template-columns:repeat\(2,minmax\(0,1fr\)\)/);
});
test('invoice and inventory operations have themed print, edit, store, link and danger accents',()=>{
 for(const cls of ['is-print','is-edit']){
  assert.ok(invoice.includes('class="'+cls+'"'));
  assert.ok(stock.includes('class="'+cls+'"'));
  assert.ok(invoiceCss.includes('button.'+cls));
  assert.ok(stockCss.includes('button.'+cls));
 }
 for(const cls of ['is-store','is-link']){
  assert.ok(stock.includes('class="'+cls+'"'));
  assert.ok(stockCss.includes('button.'+cls));
 }
 assert.match(invoiceCss,/--invoice-accent:var\(--mud-palette-error\)/);
 assert.match(stockCss,/--stock-accent:var\(--mud-palette-error\)/);
});
test('customer menu and two card actions use accessible themed tint',()=>{
 assert.match(customerCard,/IconColor="Color.Info"/);
 assert.match(customerCard,/IconColor="Color.Error"/);
 assert.match(customerCss,/var\(--customer-action-accent\) 13%/);
 assert.match(customerCss,/var\(--customer-action-accent\) 20%/);
 assert.match(customerCss,/\.customer-card-heading ::deep \.customer-more > button/);
});
