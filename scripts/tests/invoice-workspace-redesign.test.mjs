import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root=new URL('../../src/App/Client/GoldEx.Client/Pages/Invoices/Components/',import.meta.url);
const read=f=>readFileSync(new URL(f,root),'utf8');

test('invoice editor uses four free-navigation cards, no old tabs or expansion panels',()=>{
 const view=read('EditorForm.razor');
 for(const id of ['invoice-core','invoice-customer','invoice-lines','invoice-finance']){
  assert.ok(view.includes('id="'+id+'"'),id);
 }
 assert.match(view,/class="invoice-workspace-steps"/);
 assert.match(view,/class="invoice-workspace-columns"/);
 assert.match(view,/class="invoice-workspace-main"/);
 assert.match(view,/class="invoice-workspace-side"/);
 assert.doesNotMatch(view,/<MudTabs|<MudExpansionPanel|<MudCard Class="invoice-editor-shell"/);
 assert.match(view,/<MudForm Model="_model"/);
 assert.match(view,/SubmitAsync\(\)/);
 assert.match(view,/OnDelete/);
 assert.match(view,/OnPrintAsync/);
});
test('original invoice fields and validation remain attached to real model',()=>{
 const view=read('EditorForm.razor');
 for(const key of ['InvoiceType','TradeScale','InvoiceNumber','InvoiceDate','DueDate','ExchangeRate','InvoicePriceUnit']){
  assert.ok(view.includes('_model.'+key),key);
 }
 assert.match(view,/Validation="@\(_invoiceValidator.ValidateValue\)"/);
 assert.match(view,/CustomerRemaining CustomerId="customer.Id.Value"/);
 assert.match(view,/@bind-Value="_model.Customer"/);
 assert.match(view,/OnAddCustomer/);
 assert.match(view,/OnEditCustomer/);
});
test('four lightweight tabs show counts and preserve component callbacks',()=>{
 const view=read('EditorForm.razor');
 assert.equal((view.match(/role="tab"/g)||[]).length,4);
 for(const [name,callbacks] of [
  ['ProductItemsList',['OnEditProductItem','OnRemoveProductItem','OnAddProductItem','OnOpenProductSelector','OnProductBarcodeChanged','OnPrintProductBarcode']],
  ['CoinItemsList',['OnEditCoinItem','OnRemoveCoinItem','OnAddCoinItem','OnOpenCoinSelector','OnCoinBarcodeChanged','OnPrintCoinBarcode']],
  ['CurrencyItemsList',['OnEditCurrencyItem','OnRemoveCurrencyItem','OnAddCurrencyItem','OnOpenCurrencySelector']],
  ['UsedProductsList',['OnEditUsedProduct','OnRemoveUsedProduct','OnAddUsedProduct','OnPrintUsedBarcode']]
 ]){const at=view.indexOf('<'+name);assert.ok(at>=0,name);const markup=view.slice(at,view.indexOf('/>',at)+2);for(const cb of callbacks)assert.ok(markup.includes(cb),name+':'+cb);}
 const logic=read('EditorForm.razor.cs');
 assert.match(logic,/private int InvoiceItemCount/);
 assert.match(logic,/private int _activeItemTab/);
});
test('invoice stock tables are semantic desktop tables plus compact mobile item cards',()=>{
 for(const name of ['ProductItemsList','CoinItemsList','CurrencyItemsList','UsedProductsList']){
  const view=read(name+'.razor');
  assert.match(view,/<table>/,name);
  assert.match(view,/invoice-line-mobile/,name);
  assert.match(view,/invoice-line-card/,name);
  assert.match(view,/invoice-line-facts/,name);
  assert.match(view,/invoice-line-empty/,name);
  assert.doesNotMatch(view,/<MudTable/,name);
  assert.match(view,/OnAddItem/,name);
  assert.match(view,/OnEditItem/,name);
  assert.match(view,/OnRemoveItem/,name);
 }
});
test('financial sidebar always visible with live totals, all payment operations and 400/500 typography',()=>{
 const view=read('EditorForm.razor'),css=read('EditorForm.razor.css');
 for(const key of ['TotalInvoiceAmount','TotalUnpaidAmount','TotalDiscountsAmount','TotalExtraCostsAmount','TotalPaidPaymentsAmount','TotalReceivedPaymentAmount','TotalItemsTaxAmount','TotalItemsWageAmount','TotalItemsProfitAmount','TotalUsedProductsAmount']){
   assert.ok(view.includes('_model.'+key),key);
 }
 for(const cb of ['OpenPaymentsDialog(false)','OpenPaymentsDialog(true)','OpenDiscountsDialog','OpenExtraCostsDialog','OnTotalUnpaidMenuToggled']){
   assert.ok(view.includes(cb),cb);
 }
 assert.match(css,/\.invoice-workspace-side\{[^}]*position:sticky/);
 assert.match(css,/@media\(max-width:699px\)/);
 assert.match(css,/\.invoice-workspace ::deep \.invoice-line-desktop\{display:none\}/);
 assert.match(css,/\.invoice-workspace ::deep \.invoice-line-mobile\{display:grid/);
 assert.doesNotMatch(css,/font-weight:(?:700|800|900)/);
});
