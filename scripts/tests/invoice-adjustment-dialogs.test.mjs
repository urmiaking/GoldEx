import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/Invoices/Components/', import.meta.url);
const read = name => readFileSync(new URL(name, root), 'utf8');
test('invoice actions retain full pill buttons and sticky summary',()=>{
 const css=read('EditorForm.razor.css');
 assert.match(css,/\.invoice-financial-actions ::deep \.mud-button-root\{[^}]*border-radius:999px/);
 assert.match(css,/\.invoice-form-actions ::deep \.mud-button-root\{[^}]*border-radius:999px/);
 assert.match(css,/\.invoice-workspace-side\{[^}]*position:sticky/);
});
test('discount and expense use responsive modal, no old popover',()=>{
 const view=read('EditorForm.razor'), code=read('EditorForm.razor.cs'), dialog=read('InvoiceAdjustmentsDialog.razor');
 assert.doesNotMatch(view,/_discountMenuOpen|_extraCostsMenuOpen|<DiscountList|<ExtraCostList/);
 assert.doesNotMatch(code,/OnDiscountMenuToggled|OnExtraCostsMenuToggled/);
 assert.match(view,/OnClick="OpenDiscountsDialog"/);
 assert.match(view,/OnClick="OpenExtraCostsDialog"/);
 assert.match(code,/ResponsiveDialogs.Show<InvoiceAdjustmentsDialog>/);
 assert.match(dialog,/<EditorDialogFrame>/);
 assert.match(dialog,/<DiscountList/);
 assert.match(dialog,/<ExtraCostList/);
});
test('cancel leaves invoice unmodified; confirmation commits separate cloned rows',()=>{
 const code=read('EditorForm.razor.cs'), dialog=read('InvoiceAdjustmentsDialog.razor');
 assert.match(code,/_model.InvoiceDiscounts.Select\(x => new InvoiceDiscountVm/);
 assert.match(code,/_model.InvoiceExtraCosts.Select\(x => new InvoiceExtraCostVm/);
 assert.match(code,/Canceled: false, Data: List<InvoiceDiscountVm> updated/);
 assert.match(code,/Canceled: false, Data: List<InvoiceExtraCostVm> updated/);
 assert.match(dialog,/Session\?\.Cancel\(\)/);
 assert.match(dialog,/Session\?\.Close\(IsDiscount \? \(object\)Discounts : ExtraCosts\)/);
});
test('both adjustment editors retain currency conversion and responsive card layout',()=>{
 for(const name of ['DiscountList','ExtraCostList']){
  const view=read(name+'.razor');
  assert.doesNotMatch(view,/<MudTable/);
  assert.match(view,/class="invoice-adjustments-row"/);
  assert.match(view,/@bind-Value="item.Amount"/);
  assert.match(view,/@bind-Value="item.Description"/);
  assert.match(view,/@bind-Value="item.ExchangeRate"/);
  assert.match(view,/SelectPriceUnit\(price, item\)/);
  assert.match(view,/Items.Sum\(x => x.Amount \* \(x.ExchangeRate \?\? 1\)\)/);
 }
 assert.match(read('DiscountList.razor'),/OnTotalRemainingClicked/);
 assert.match(read('InvoiceAdjustmentsDialog.razor.css'),/@media\(max-width:599px\)/);
});