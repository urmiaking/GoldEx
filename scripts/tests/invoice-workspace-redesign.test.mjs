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
 assert.match(view,/class="invoice-workspace-top"/);
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
 assert.match(css,/\.invoice-workspace-top\{[^}]*display:grid/);
 assert.match(css,/@media\(max-width:699px\)/);
 assert.match(css,/\.invoice-workspace ::deep \.invoice-line-desktop\{display:none\}/);
 assert.match(css,/\.invoice-workspace ::deep \.invoice-line-mobile\{display:grid/);
 assert.doesNotMatch(css,/font-weight:(?:700|800|900)/);
});
test('finance owns the left sidebar; header and customer sit above items in the right column',()=>{
 const view=read('EditorForm.razor');
 const css=read('EditorForm.razor.css');
 const skeleton=read('EditorFormSkeleton.razor');
 const skeletonCss=read('EditorFormSkeleton.razor.css');
 const columns=view.indexOf('<div class="invoice-workspace-columns">');
 const main=view.indexOf('<main class="invoice-workspace-main">');
 const top=view.indexOf('<div class="invoice-workspace-top">');
 const core=view.indexOf('<section id="invoice-core"');
 const customer=view.indexOf('<section id="invoice-customer"');
 const lines=view.indexOf('<section id="invoice-lines"');
 const mainEnd=view.indexOf('</main>',main);
 const aside=view.indexOf('<aside id="invoice-finance"');
 const finance=view.indexOf('invoice-financial-card',aside);
 const finalReview=view.indexOf('invoice-final-review',aside);
 assert.ok(columns>0 && columns<main && main<top && top<core && core<customer && customer<lines && lines<mainEnd && mainEnd<aside && aside<finance && finance<finalReview);
 assert.match(view,/<div class="invoice-workspace-columns">\s*<main class="invoice-workspace-main">\s*<div class="invoice-workspace-top">/);
 assert.match(view,/<\/main>\s*<aside id="invoice-finance"/);
 assert.match(css,/\.invoice-workspace-columns\{display:grid;grid-template-columns:minmax\(0,2fr\) minmax\(335px,1fr\)/);
 assert.match(css,/\.invoice-workspace-top\{display:grid;grid-template-columns:minmax\(0,3fr\) minmax\(195px,1fr\)/);
 assert.match(css,/\.invoice-workspace-side\{[^}]*position:sticky/);
 assert.match(css,/\.invoice-customer-details\{display:grid;grid-template-columns:minmax\(0,1fr\)/);
 assert.match(css,/@media\(max-width:1099\.98px\)\{\s*\.invoice-workspace-columns\{grid-template-columns:minmax\(0,1fr\)/);
 assert.match(css,/\.invoice-core-field-caption\{display:none\}/);
 const skMain=skeleton.indexOf('<main class="invoice-workspace-main">');
 const skCore=skeleton.indexOf('invoice-core-section'),skCustomer=skeleton.indexOf('invoice-customer-section');
 const skLines=skeleton.indexOf('invoice-lines-section'),skAside=skeleton.indexOf('<aside class="invoice-workspace-side">');
 assert.ok(skMain<skCore && skCore<skCustomer && skCustomer<skLines && skLines<skAside);
 assert.match(skeletonCss,/\.invoice-workspace-columns\{display:grid;grid-template-columns:minmax\(0,2fr\) minmax\(335px,1fr\)/);
});
