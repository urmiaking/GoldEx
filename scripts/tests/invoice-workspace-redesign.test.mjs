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
 for(const cb of ['OpenPaymentsDialog(false)','OpenPaymentsDialog(true)','OpenDiscountsDialog','OpenExtraCostsDialog','OpenUnpaidUnitDialog']){
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
 assert.ok(columns>0 && columns<main && main<top && top<core && core<customer && customer<lines && lines<mainEnd && mainEnd<aside && aside<finance && view.indexOf('invoice-final-review',aside)===-1);
 assert.match(view,/<div class="invoice-workspace-columns">\s*<main class="invoice-workspace-main">\s*<div class="invoice-workspace-top">/);
 assert.match(view,/<\/main>\s*<aside id="invoice-finance"/);
 assert.match(css,/\.invoice-workspace-columns\{display:grid;grid-template-columns:minmax\(0,3fr\) minmax\(315px,1fr\);grid-template-areas:"main finance"/);
 assert.match(css,/\.invoice-workspace-top\{display:grid;grid-template-columns:minmax\(0,2fr\) minmax\(0,1fr\)/);
 assert.match(css,/\.invoice-workspace-side\{[^}]*position:sticky/);
 assert.match(view,/class="invoice-customer-identity"/);
 assert.match(css,/@media\(max-width:1299\.98px\)\{\s*\.invoice-workspace-columns\{grid-template-columns:minmax\(0,1fr\);grid-template-areas:"main" "finance"/);
 assert.match(css,/\.invoice-core-field-caption\{display:none\}/);
 const skMain=skeleton.indexOf('<main class="invoice-workspace-main">');
 const skCore=skeleton.indexOf('invoice-core-section'),skCustomer=skeleton.indexOf('invoice-customer-section');
 const skLines=skeleton.indexOf('invoice-lines-section'),skAside=skeleton.indexOf('<aside class="invoice-workspace-side">');
 assert.ok(skMain<skCore && skCore<skCustomer && skCustomer<skLines && skLines<skAside);
 assert.match(skeletonCss,/\.invoice-workspace-columns\{display:grid;grid-template-columns:minmax\(0,3fr\) minmax\(315px,1fr\)/);
});
test('desktop columns use the exact 50% header, 25% customer, 25% finance proportions',()=>{
 const css=read('EditorForm.razor.css');
 const skeletonCss=read('EditorFormSkeleton.razor.css');
 assert.match(css,/\.invoice-workspace-columns>\.invoice-workspace-side\{grid-area:finance;justify-self:stretch;min-width:0;width:100%\}/);
 assert.match(css,/\.invoice-workspace-columns>\.invoice-workspace-side>\.invoice-settlement-main\{width:100%;grid-template-columns:minmax\(0,1fr\)\}/);
 assert.match(css,/\.invoice-workspace-top>\.invoice-section\{min-width:0;height:100%\}/);
 assert.match(css,/\.invoice-customer-tools\{[^}]*margin-bottom:13px/);
 assert.match(skeletonCss,/grid-template-areas:"main finance"/);
 assert.doesNotMatch(css,/invoice-core-section\{min-height:285px\}/);
});
test('invoice steps navigate using a lazy-loaded module and show active step',()=>{
 const view=read('EditorForm.razor');
 const code=read('EditorForm.razor.cs');
 const css=read('EditorForm.razor.css');
 const js=readFileSync(new URL('../../../wwwroot/js/invoice-workspace.js',root),'utf8');
 for(let i=1;i<=4;i++) assert.match(view,new RegExp('ScrollToInvoiceSectionAsync\\('+i+'\\)'));
 assert.match(view,/@onclick:preventDefault="true"/);
 assert.match(view,/aria-current="@\(_activeInvoiceStep == 1/);
 assert.match(code,/InvokeAsync<IJSObjectReference>\("import", "\.\/js\/invoice-workspace\.js"\)/);
 assert.match(code,/InvokeVoidAsync\("scrollToSection", sectionId\)/);
 assert.match(css,/\.invoice-workspace-steps a\.is-active/);
 assert.match(js,/export function scrollToSection\(sectionId\)/);
 assert.match(js,/scrollIntoView\(/);
});
test('animated accounting accordion, modal balance unit selector and no final review card',()=>{
 const view=read('EditorForm.razor'),code=read('EditorForm.razor.cs');
 const css=read('EditorForm.razor.css'),skeleton=read('EditorFormSkeleton.razor');
 const details=view.indexOf('class="invoice-financial-accordion @(');
 const breakdown=view.indexOf('class="invoice-financial-breakdown"');
 assert.ok(details>0 && breakdown>details);
 assert.match(view,/aria-expanded="@_showFinancialDetails"/);
 assert.match(view,/@onclick="ToggleFinancialDetails"/);
 assert.match(view,/aria-hidden="@\(!_showFinancialDetails\)"/);
 assert.match(css,/grid-template-rows:0fr/);
 assert.match(css,/grid-template-rows:1fr/);
 assert.match(css,/prefers-reduced-motion:reduce/);
 assert.match(view,/OnClick="OpenUnpaidUnitDialog"/);
 assert.doesNotMatch(view,/<MudPopover/);
 assert.match(code,/ResponsiveDialogs.Show<UnpaidBalanceUnitDialog>/);
 assert.match(code,/Data: UnpaidBalanceUnitDialog.UnitSelection updated/);
 assert.doesNotMatch(view,/invoice-final-review|آماده ثبت/);
 assert.doesNotMatch(skeleton,/invoice-final-review/);
});
test('compact customer, clear page header, colored statuses and counts',()=>{
 const view=read('EditorForm.razor'),css=read('EditorForm.razor.css');
 const code=read('EditorForm.razor.cs');
 assert.doesNotMatch(view,/مدیریت فاکتور \/|اطلاعات، اقلام و تسویه را/);
 assert.match(view,/IsEditMode \? "ویرایش فاکتور" : "ثبت فاکتور"/);
 assert.match(view,/invoice-type-chip/);
 assert.match(css,/\.invoice-type-chip.is-purchase/);
 assert.match(css,/\.invoice-type-chip.is-sale/);
 assert.match(css,/\.invoice-workspace-state.is-pending/);
 assert.doesNotMatch(view,/بخش [۱۲۳۴]/);
 assert.match(view,/customer\?\.FullName/);
 assert.match(view,/<span class="invoice-customer-id">\(@customer.NationalId\)<\/span>/);
 assert.match(view,/class="invoice-customer-phone"/);
 assert.doesNotMatch(view,/customer.CustomerType.GetDisplayName/);
 assert.doesNotMatch(view,/<MudSwitch/);
 assert.doesNotMatch(code,/_applyCurrentInvoice/);
 assert.match(view,/AddedValue="@\(IsEditMode \? GetCustomerBalanceChange\(\) : GetNewInvoiceCustomerBalanceEffect\(\)\)"/);
 assert.match(view,/InvoiceDiscounts.Count/);
 assert.match(view,/InvoiceExtraCosts.Count/);
 assert.match(view,/Color="Color.Success" Variant="Variant.Outlined"/);
});
test('mobile barcode stays beside add button and skeleton keeps labels',()=>{
 const css=read('EditorForm.razor.css'),skeleton=read('EditorFormSkeleton.razor');
 const skeletonCss=read('EditorFormSkeleton.razor.css');
 assert.match(css,/:has\(\.mud-input-control\)/);
 assert.match(css,/grid-template-columns:minmax\(0,1fr\) auto/);
 assert.match(skeleton,/>مشخصات فاکتور<\/h2>/);
 assert.match(skeleton,/>مالی و تسویه<\/h2>/);
 assert.match(skeleton,/>جزئیات محاسبات<\/span>/);
 assert.match(skeletonCss,/invoice-skeleton-label/);
});
test('responsive unpaid-unit dialog previews and safely commits only after confirmation',()=>{
 const dialog=read('UnpaidBalanceUnitDialog.razor');
 const css=read('UnpaidBalanceUnitDialog.razor.css');
 const editor=read('EditorForm.razor.cs');
 const selector=read('UnpaidPriceSelector.razor');
 assert.match(dialog,/<EditorDialogFrame>/);
 assert.match(dialog,/این تغییر مبلغ اصلی فاکتور یا پرداخت‌های ثبت‌شده را تغییر نمی‌دهد/);
 assert.match(dialog,/UnpaidPriceUnitChanged="OnUnitChanged"/);
 assert.match(dialog,/ExchangeRateChanged="OnRateChanged"/);
 assert.match(dialog,/_selectedRate = null/);
 assert.match(dialog,/Disabled="@InvalidRate"/);
 assert.match(dialog,/Session\?\.Cancel\(\)/);
 assert.match(dialog,/Session\?\.Close\(new UnitSelection/);
 assert.match(editor,/if \(await dialog.Result is \{ Canceled: false, Data: UnpaidBalanceUnitDialog.UnitSelection updated \}\)/);
 assert.match(css,/@media\(max-width:600px\)/);
 assert.match(selector,/واحد نمایش مانده/);
});
test('compact customer always previews current invoice without the optional toggle',()=>{
 const view=read('EditorForm.razor'),cs=read('EditorForm.razor.cs');
 assert.doesNotMatch(view,/invoice-customer-balance-option|MudSwitch/);
 assert.doesNotMatch(cs,/_applyCurrentInvoice|RemainingTooltipText/);
 assert.match(view,/AddedValue="@\(IsEditMode \? GetCustomerBalanceChange\(\) : GetNewInvoiceCustomerBalanceEffect\(\)\)"/);
 assert.match(view,/ToStringFunc="@\(customer => customer\?\.FullName\)"/);
});
