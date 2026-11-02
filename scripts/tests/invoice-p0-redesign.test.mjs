import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/', import.meta.url);
const read = file => readFileSync(new URL(file,root),'utf8');

test('invoice payment overview uses responsive cards/table and native modal', () => {
 const view=read('Invoices/Components/PaymentDialog.razor');
 const logic=read('Invoices/Components/PaymentDialog.razor.cs');
 assert.match(view,/<EditorDialogFrame>/);
 assert.doesNotMatch(view,/<MudTable|<MudDialog/);
 assert.match(view,/invoice-payment-mobile/);
 assert.match(view,/invoice-payment-table/);
 for(const p of ['InternalCash','CustomerTransfer','Check','UsedGoldInventory','MoltenGoldInventory','Coin'])
   assert.match(view,new RegExp('PaymentType\\.'+p+'\\)'));
 assert.doesNotMatch(view,/OnApplyPrepaymentVouchers|PaymentVouchersSelectorList/);
 assert.match(view,/PaymentType is not PaymentType.TransferedPayment/);
 assert.match(logic,/CurrentRemaining \+ GetBalanceEffect\(InvoiceType, payment\)/);
 assert.match(logic,/GetRemainingBefore\(payment\)/);
 assert.match(logic,/Session.Close\(Items\)/);
 assert.match(logic,/ResponsiveDialogs.Show<InvoicePaymentDeleteDialog>/);
 assert.match(logic,/ResponsiveDialogs.Show<PaymentEditor>/);
});
test('payment editor retains financial validation, image upload, balance math and native nested forms', () => {
 const view=read('Invoices/Components/PaymentEditor.razor');
 const logic=read('Invoices/Components/PaymentEditor.razor.cs');
 assert.match(view,/<EditorDialogFrame>/);
 assert.doesNotMatch(view,/<MudDialog/);
 assert.match(view,/MudFileUpload/);
 assert.match(view,/invoice-payment-date/);
 assert.match(logic,/_form.ValidateAsync/);
 assert.match(logic,/Session.Close\(Model\)/);
 assert.match(logic,/Model.CheckImageFile/);
 assert.match(logic,/ApplyTotalRemaining/);
 assert.match(logic,/ResponsiveDialogs.Show<FinancialAccountEditor>/);
 assert.match(logic,/ResponsiveDialogs.Show<Customers.Components.Editor>/);
});
test('invoice stock picker keeps selection and calculation service contract', () => {
 const view=read('Invoices/Components/InventoryItemSelector.razor');
 const logic=read('Invoices/Components/InventoryItemSelector.razor.cs');
 assert.match(view,/<EditorDialogFrame>/);
 assert.match(view,/Selectable="true"/);
 assert.match(view,/SelectedItemsChanged="OnSelectedItemsChanged"/);
 assert.match(view,/invoice-stock-picker-selected/);
 assert.match(logic,/Task.WhenAll\(productItemTasks\)/);
 assert.match(logic,/Task.WhenAll\(coinItemTasks\)/);
 assert.match(logic,/Task.WhenAll\(currencyItemTasks\)/);
 assert.match(logic,/Session.Close\(data\)/);
 assert.match(logic,/GetCurrencyFinancialAccountAsync/);
 assert.match(read('InventoryStocks/Components/InventoryStockList.razor'),/RowsPerPage="@\(Selectable \? 10 : 14\)"/);
});
test('all invoice item editors use native dialogs and preserve validation and models', () => {
 for(const name of ['ProductItemEditor','CoinItemEditor','CurrencyItemEditor','UsedProductEditor']){
  const base='Invoices/Components/'+name;
  const view=read(base+'.razor'),logic=read(base+'.razor.cs');
  assert.match(view,/<EditorDialogFrame>/,name);
  assert.doesNotMatch(view,/<MudDialog/,name);
  assert.match(logic,/ResponsiveDialogSession/,name);
  assert.match(logic,/Session.Close\(Model\)/,name);
  assert.match(view,/MudForm/,name);
 }
});
test('invoice main form wires new native dialogs and positive-only item removal', () => {
 const code=read('Invoices/Components/EditorForm.razor.cs');
 for(const name of ['PaymentDialog','InventoryItemSelector','ProductItemEditor','CoinItemEditor','CurrencyItemEditor','UsedProductEditor','InvoiceItemDeleteDialog'])
  assert.ok(code.includes('ResponsiveDialogs.Show<'+name+'>'),name);
 assert.match(code,/Data: true/);
 assert.match(read('Invoices/Components/InvoiceItemDeleteDialog.razor'),/<EditorDialogFrame>/);
 assert.match(read('Invoices/Components/InvoicePaymentDeleteDialog.razor'),/<EditorDialogFrame>/);
 assert.doesNotMatch(read('Invoices/SetInvoice.razor'),/<MudBreadcrumbs/);
});
test('prepayment selection keeps prior selection, multi-select and save data shape',()=>{
 const view=read('Finances/PaymentVouchers/Components/PaymentVouchersSelectorList.razor');
 const logic=read('Finances/PaymentVouchers/Components/PaymentVouchersSelectorList.razor.cs');
 assert.match(view,/<EditorDialogFrame>/);
 assert.doesNotMatch(view,/<MudTable/);
 assert.match(view,/invoice-prepayment-card/);
 assert.match(view,/ToggleItem\(item,v\)/);
 assert.match(logic,/SelectedPaymentVouchers.Contains\(voucher.Id\)/);
 assert.match(logic,/Session.Close\(_selectedItems\)/);
});
test('payment descriptions get a wide column and wrap instead of being ellipsized', () => {
 const view = read('Invoices/Components/PaymentDialog.razor');
 const styles = read('Invoices/Components/PaymentDialog.razor.css');
 assert.match(view, /<col class="invoice-payment-col-description"\s*\/>/);
 assert.match(view, /title="@item.Description"/);
 assert.match(styles, /\.invoice-payment-col-description\{width:38%\}/);
 assert.match(styles, /\.invoice-payment-description\{[^}]*white-space:normal;[^}]*overflow-wrap:anywhere/);
 assert.doesNotMatch(styles, /max-width:180px|text-overflow:ellipsis/);
 assert.match(styles, /\.invoice-payment-mobile\{display:none\}/);
});