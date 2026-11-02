import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
const read = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const root = 'src/App/Client/GoldEx.Client/Pages/Customers/Components/';

test('invoice editor opens customer in shared responsive dialog', () => {
    const src = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/EditorForm.razor.cs');
    const block = src.split('private async Task OnAddCustomer()')[1].split('private decimal GetNewInvoiceCustomerBalanceEffect()')[0];
    assert.equal((block.match(/ResponsiveDialogs\.Show<Customers\.Components\.Editor>/g) || []).length, 2);
    assert.doesNotMatch(block, /DialogService\.ShowAsync/);
    assert.match(block, /ReturnModel/);
});

test('customer invoice dialog shows the original invoice table on desktop and cards on mobile', () => {
    const view = read(root + 'CustomerHistoryDialog.razor');
    const css = read(root + 'CustomerHistoryDialog.razor.css');
    const invoice = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
    assert.match(view, /class="gex-customer-history-content"/);
    assert.doesNotMatch(view, /<EditorDialogFrame>/);
    assert.match(css, /\.gex-customer-history\s*\{[^}]*display:flex;[^}]*min-height:0/s);
    assert.match(css, /@media\(min-width:960px\)\s*\{/);
    assert.match(css, /\.invoice-desktop-table\s*\{\s*display:block/);
    assert.match(css, /\.invoice-mobile-list\s*\{\s*display:none/);
    assert.match(css, /@media\(max-width:959\.98px\)/);
    assert.match(css, /\.invoice-mobile-list\s*\{\s*display:grid/);
    assert.match(css, /\.gex-customer-history-content\s*\{[^}]*overflow-y:auto/s);
    assert.match(css, /\.invoice-card-amounts \.invoice-paid\s*\{[^}]*success-text/);
    assert.doesNotMatch(css, /\.invoice-card-amounts > div > span\s*\{/);
    assert.match(invoice, /!firstRender \|\| CustomerId\.HasValue/);
});

test('customer ledger is an unpaginated accounting timeline with all filtered records', () => {
    const view = read(root + 'CustomerLedgerDirectory.razor');
    const logic = read(root + 'CustomerLedgerDirectory.razor.cs');
    const css = read(root + 'CustomerLedgerDirectory.razor.css');
    assert.match(view, /<ol class="customer-ledger-timeline"/);
    assert.match(view, /<li class="customer-ledger-event/);
    assert.match(view, /class="customer-ledger-amount/);
    assert.doesNotMatch(view, /<MudTable|customer-ledger-pagination|NextAsync|PreviousAsync/);
    assert.match(logic, /TimelineBatchSize = 100/);
    assert.match(logic, /GetListAsync\(/);
    assert.match(logic, /new RequestFilter\(offset, TimelineBatchSize, search/);
    assert.match(logic, /offset \+= batch\.Count;/);
    assert.match(logic, /while \(offset < total\)/);
    assert.match(logic, /CustomerId, _priceUnitId/);
    assert.match(logic, /GetAvailablePriceUnitsAsync/);
    assert.match(css, /\.customer-ledger-timeline::before/);
    assert.match(css, /\.customer-ledger-event\.is-credit \.customer-ledger-node/);
    assert.match(css, /@media\(max-width:680px\)/);
});

test('customer edit/create retains fields and modern spacing', () => {
    const view = read(root + 'Editor.razor');
    assert.match(view, /gex-customer-editor-intro/);
    for (const field of ['CustomerType','FullName','NationalId','PhoneNumber','Address'])
        assert.match(view, new RegExp('Model\\.' + field));
    assert.match(view, /Variant="Variant\.Outlined">انصراف/);
});
