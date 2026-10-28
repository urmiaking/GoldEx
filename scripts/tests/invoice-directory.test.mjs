import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

function read(path) {
    return readFileSync(new URL(`../../${path}`, import.meta.url), 'utf8');
}

const list = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const code = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
const detail = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor');
const styles = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
const observerSource = read('src/App/Server/GoldEx.Server/wwwroot/js/invoice-details.js');

test('desktop invoices use semantic native table, mobile uses standalone cards', () => {
    assert.match(list, /<table class="invoice-table">/);
    assert.match(list, /<thead>/);
    assert.match(list, /<th scope="col">/);
    assert.match(list, /<div class="invoice-mobile-list">/);
    assert.match(list, /<article @key="invoice.Id" class="invoice-card/);
    assert.match(styles, /@media \(max-width: 959\.98px\)/);
    assert.match(styles, /\.invoice-desktop-table\s*\{\s*display: none/);
    assert.doesNotMatch(list, /MudTable|MudPagination|MudCollapse|MudCard/);
});

test('sorting, date, text, type and status filters remain wired to the API, not applied only to visible rows', () => {
    assert.match(code, /service\.GetListAsync\(filter, invoiceFilter, CustomerId, token\)/);
    assert.match(code, /new RequestFilter\(_pageIndex \* _pageSize, _pageSize, _searchString,/);
    for (const name of ['ChangeSort', 'ChangeType', 'ChangeScale', 'ChangeStatus', 'OnSearchInput', 'OnDateRangeChanged', 'OpenMobileFilters'])
        assert.match(list, new RegExp(name));
    for (const query of ['page', 'size', 'search', 'status', 'type', 'scale', 'start', 'end'])
        assert.match(code, new RegExp(`"${query}"`));
    assert.doesNotMatch(code, /MudTable|TableState|TableData/);
});

test('invoice operation and detail actions are still accessible on both views', () => {
    for (const fn of ['OnViewInvoice(invoice)', 'OnEditInvoice(invoice)', 'OnRemoveInvoice(invoice)', 'ToggleDetails(invoice)'])
        assert.equal(list.split(fn).length - 1, fn === 'ToggleDetails(invoice)' ? 3 : 2, fn);
    assert.match(detail, /<table class="invoice-details-table">/);
    assert.equal(detail.split('<table class="invoice-details-table">').length - 1, 2);
    assert.doesNotMatch(detail, /MudTable|MudPagination/);
    assert.match(detail, /CustomerRemaining/);
});

test('only visible desktop/mobile detail instance fetches, and observer disposes cleanly', async () => {
    const instances = [];
    class IntersectionObserverMock {
        constructor(callback) { this.callback = callback; this.stopped = false; instances.push(this); }
        observe(node) { this.node = node; }
        disconnect() { this.stopped = true; }
    }
    const context = { IntersectionObserver: IntersectionObserverMock };
    vm.runInNewContext(observerSource.replace('export function ', 'function ') + '\nthis.api = observeVisible;', context);

    let calls = 0;
    const ref = { invokeMethodAsync(name) {
        assert.equal(name, 'ActivateAsync');
        calls++;
        return Promise.resolve();
    } };
    const shown = context.api({ id: 'shown' }, ref);
    const hidden = context.api({ id: 'hidden' }, ref);
    assert.equal(instances.length, 2);
    instances[0].callback([{ isIntersecting: true }]);
    await Promise.resolve();
    assert.equal(calls, 1);
    assert.equal(instances[0].stopped, true);
    instances[1].callback([{ isIntersecting: false }]);
    assert.equal(calls, 1);
    hidden.dispose();
    instances[1].callback([{ isIntersecting: true }]);
    assert.equal(calls, 1);
    shown.dispose();
});
