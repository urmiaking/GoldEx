import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
const read = path => readFileSync(new URL('../../' + path, import.meta.url),'utf8');
const root='src/App/Client/GoldEx.Client/Pages/Customers/Components/';

test('invoice editor opens customer in shared responsive dialog',()=>{
 const src=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/EditorForm.razor.cs');
 const block=src.split('private async Task OnAddCustomer()')[1].split('private decimal GetNewInvoiceCustomerBalanceEffect()')[0];
 assert.equal((block.match(/ResponsiveDialogs\.Show<Customers\.Components\.Editor>/g)||[]).length,2);
 assert.doesNotMatch(block,/DialogService\.ShowAsync/);
 assert.match(block,/ReturnModel/);
});
test('customer invoices show bounded cards without the nested frame',()=>{
 const view=read(root+'CustomerHistoryDialog.razor');
 const css=read(root+'CustomerHistoryDialog.razor.css');
 const invoice=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
 assert.match(view,/class="gex-customer-history-content"/);
 assert.doesNotMatch(view,/<EditorDialogFrame>/);
 assert.match(css,/\.gex-customer-history\s*\{[^}]*display:flex;[^}]*min-height:0/s);
 assert.match(css,/\.invoice-mobile-list\s*\{\s*display:grid/);
 assert.match(css,/\.invoice-desktop-table\s*\{\s*display:none/);
 assert.match(css,/\.gex-customer-history-content\s*\{[^}]*overflow-y:auto/s);
 assert.match(invoice,/!firstRender \|\| CustomerId\.HasValue/);
});
test('customer ledger retains server filters and native table/mobile cards',()=>{
 const view=read(root+'CustomerLedgerDirectory.razor');
 const logic=read(root+'CustomerLedgerDirectory.razor.cs');
 const css=read(root+'CustomerLedgerDirectory.razor.css');
 assert.match(view,/class="customer-ledger-table"/);
 assert.match(view,/class="customer-ledger-cards"/);
 assert.doesNotMatch(view,/<MudTable/);
 assert.match(logic,/GetListAsync\(Filter, request, ct\)/);
 assert.match(logic,/GetAvailablePriceUnitsAsync/);
 assert.match(logic,/PageSize = 10/);
 assert.match(css,/@media\(max-width:699px\)/);
 assert.match(css,/\.customer-ledger-table-scroll\s*\{\s*display:none/);
 assert.match(css,/\.customer-ledger-cards\s*\{\s*display:grid/);
});
test('customer edit/create retains fields and modern spacing',()=>{
 const view=read(root+'Editor.razor');
 assert.match(view,/gex-customer-editor-intro/);
 for(const field of ['CustomerType','FullName','NationalId','PhoneNumber','Address']) assert.match(view,new RegExp('Model\\.'+field));
 assert.match(view,/Variant="Variant\.Outlined">انصراف/);
});
