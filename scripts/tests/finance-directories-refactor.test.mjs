import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/', import.meta.url);
const read = file => readFileSync(new URL(file,root),'utf8');

test('transfer mobile cards are compact, information-first and no arrow icons', () => {
 const razor = read('Finances/CustomerTransfers/Components/CustomerTransferList.razor');
 const css = read('Finances/CustomerTransfers/Components/CustomerTransferList.razor.css');
 const card = razor.slice(razor.indexOf('<article class="transfer-card"'));
 assert.match(card,/transfer-card-route/);
 assert.match(card,/transfer-card-money/);
 assert.match(card,/SourceCustomerName/);
 assert.match(card,/DestinationCustomerName/);
 assert.doesNotMatch(card,/transfer-flow|Icons\.Material\.Filled\.ArrowDownward|Icons\.Material\.Filled\.ArrowUpward/);
 assert.match(css,/transfer-toolbar ::deep \.transfer-sort/);
 assert.match(css,/font-weight:500/);
});
test('deletion UI for customer, invoice and product uses native responsive confirmation but keeps service call', () => {
 for(const [folder,service] of [['Customers','ICustomerService'],['Invoices','IInvoiceService'],['Products','IInventoryStockService']]){
  const view=read(folder+'/Components/Remove.razor');
  const logic=read(folder+'/Components/Remove.razor.cs');
  assert.match(view,/<EditorDialogFrame>/);
  assert.match(view,/gex-delete-confirm/);
  assert.match(logic,/ResponsiveDialogSession/);
  assert.ok(logic.includes('SendRequestAsync<'+service+'>'));
 }
 for(const f of ['Customers/Components/CustomersList.razor.cs','Invoices/Components/InvoicesList.razor.cs','InventoryStocks/Components/InventoryDirectory.razor.cs'])
  assert.match(read(f),/ResponsiveDialogs.Show<Remove>/);
});
test('payment vouchers and checks provide semantic desktop table + mobile cards, server paging and filters', () => {
 for(const [folder,name,service] of [
  ['PaymentVouchers','PaymentVouchersList','IPaymentVoucherService'],
  ['Checks','CheckPaymentsList','ICheckPaymentService']
 ]){
  const base='Finances/'+folder+'/Components/'+name;
  const view=read(base+'.razor'), code=read(base+'.razor.cs'),css=read(base+'.razor.css');
  assert.match(view,/<table class="finance-table"/);
  assert.match(view,/class="finance-mobile"/);
  assert.match(view,/class="finance-card(?:\s|")/);
  assert.doesNotMatch(view,/<MudTable/);
  assert.match(view,/finance-filter-button/);
  assert.match(code,new RegExp('SendRequestAsync<'+service));
  assert.match(code,/new RequestFilter\(_pageIndex \* _pageSize/);
  assert.match(code,/cancelPrevious: true/);
  assert.match(code,/ResponsiveDialogs.Show/);
  assert.match(css,/finance-card-main/);
  assert.match(css,/max-width:959\.98px/);
  assert.match(css,/font-weight:500/);
 }
});
test('checks retain financial settlement actions and responsive dialogs', () => {
 const base='Finances/Checks/Components/';
 const code=read(base+'CheckPaymentsList.razor.cs');
 for(const name of ['AcceptCheckDialog','ReturnCheckDialog','ViewCheckDialog','CheckPaymentFiltersDialog']){
  assert.ok(code.includes('ResponsiveDialogs.Show<'+name+'>'));
  assert.match(read(base+name+'.razor'),/<EditorDialogFrame>/);
  assert.match(read(base+name+'.razor.cs'),/ResponsiveDialogSession/);
 }
 assert.match(read(base+'AcceptCheckDialog.razor.cs'),/AcceptCheckPaymentRequest/);
 assert.match(read(base+'ReturnCheckDialog.razor.cs'),/ReturnCheckPaymentRequest/);
});
test('payments preserve accounting writes and keep nested account/customer dialogs native', () => {
 const base='Finances/PaymentVouchers/Components/';
 const list=read(base+'PaymentVouchersList.razor.cs');
 const editor=read(base+'Editor.razor.cs');
 assert.match(editor,/service\.UpdateAsync/);
 assert.match(editor,/service\.CreateAsync/);
 assert.match(editor,/ResponsiveDialogs.Show<Customers\.Components\.Editor>/);
 assert.match(editor,/ResponsiveDialogs.Show<FinancialAccountEditor>/);
 assert.match(read(base+'Editor.razor'),/<EditorDialogFrame>/);
 assert.match(list,/PaymentVoucherDeleteDialog/);
 assert.match(read(base+'PaymentVoucherDeleteDialog.razor'),/<EditorDialogFrame>/);
});