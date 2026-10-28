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
const customerList = read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor');
const customerStyles = read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor.css');

test('desktop invoices use semantic native table, mobile uses standalone cards', () => {
    assert.match(list, /<table class="invoice-table mud-typography-body2">/);
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
    for (const name of ['ChangeSort', 'OnSortChanged', 'ToggleSortDirection', 'OnSearch', 'OpenMobileFilters'])
        assert.match(list, new RegExp(name));
    for (const query of ['page', 'size', 'search', 'status', 'type', 'scale', 'start', 'end'])
        assert.match(code, new RegExp(`"${query}"`));
    assert.doesNotMatch(code, /MudTable|TableState|TableData/);
});

test('invoice operation and detail actions are still accessible on both views', () => {
    for (const fn of ['OnViewInvoice(invoice)', 'OnEditInvoice(invoice)', 'OnRemoveInvoice(invoice)', 'ToggleDetails(invoice)'])
        assert.equal(list.split(fn).length - 1, 2, fn);
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


test('invoice heading and sticky filter toolbar use exactly the same building blocks as Customers', () => {
    for (const selector of ['directory-heading', 'directory-title', 'directory-count',
        'directory-filters', 'directory-toolbar', 'directory-search', 'directory-filter',
        'directory-sort', 'directory-sort-toggle', 'directory-applied-filters']) {
        assert.match(customerList, new RegExp('(?:class|Class)="' + selector + '(?: [^"]*)?"'));
        assert.match(list, new RegExp('(?:class|Class)="' + selector + '(?: [^"]*)?"'));
    }
    for (const tag of ['MudTextField', 'MudButton', 'MudSelect', 'MudIconButton']) {
        assert.match(customerList, new RegExp('<' + tag + '\\b'));
        assert.match(list, new RegExp('<' + tag + '\\b'));
    }
    assert.match(styles, /\.directory-filters\s*\{[^}]*position:\s*sticky;/);
    assert.match(customerStyles, /\.directory-filters\s*\{[^}]*position:\s*sticky;/);
    assert.match(styles, /\.directory-toolbar\s*\{[^}]*grid-template-columns:\s*minmax\(160px, 1fr\) auto minmax\(150px, 200px\) 44px/);
    assert.match(styles, /@media \(max-width: 959px\)\s*\{[^}]*\.directory-toolbar\s*\{[^}]*grid-template-columns:\s*minmax\(0, 1fr\) 44px/);
});

test('date, status, type, and scale filters are kept in one dialog and not misaligned toolbar dropdowns', () => {
    assert.match(list, /OnClick="OpenMobileFilters"/);
    assert.doesNotMatch(list, /<select[^>]*aria-label="(?:نوع معامله|نوع فاکتور|وضعیت پرداخت)"/);
    const dialog = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor');
    assert.match(dialog, /MudDateRangePicker/);
    for (const type of ['InvoiceType', 'TradeScale', 'InvoicePaymentStatus'])
        assert.match(dialog, new RegExp('MudSelect T="' + type + '\\?"'));
});

test('pagination is reduced to previous / current page / next and optional page size', () => {
    assert.match(list, /class="invoice-pagination-actions"/);
    assert.match(list, /@\(!CanPrevious\)/);
    assert.match(list, /@\(!CanNext\)/);
    assert.match(list, /class="invoice-page-size"/);
    assert.doesNotMatch(list, /invoice-pagination-count|@StartItem|@EndItem|نمایش .* تا .* از/);
});