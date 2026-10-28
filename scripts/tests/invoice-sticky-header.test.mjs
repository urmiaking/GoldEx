import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

function source(path) {
    return readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
}
const sheet = source('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
const customerSheet = source('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor.css');
const code = source('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
const markup = source('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const js = source('src/App/Server/GoldEx.Server/wwwroot/js/invoice-sticky-header.js');
const shell = source('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

test('document-scrolling native invoice table has no clipping ancestor and sticky header follows filters', () => {
    assert.match(sheet, /\.invoice-results \{[^}]*overflow: visible/);
    assert.match(sheet, /\.invoice-desktop-table \{[^}]*overflow: visible/);
    assert.doesNotMatch(sheet, /\.invoice-desktop-table \{[^}]*overflow-x:\s*auto/);
    assert.match(sheet, /\.invoice-table th \{[^}]*position: sticky/);
    assert.match(sheet, /top: calc\(var\(--gex-appbar-height[^;]*var\(--invoice-filters-block-size,\s*96px\)/);
    assert.match(markup, /@ref="_invoiceDirectory"/);
    assert.match(markup, /@ref="_invoiceFilters"/);
    assert.match(code, /trackFilterHeight/);
    assert.match(code, /await _stickyTracker\.InvokeVoidAsync\("dispose"\)/);
    assert.match(sheet, /@media \(max-width: 959\.98px\)[\s\S]*?\.invoice-desktop-table \{ display: none/);
});

test('filter ResizeObserver tracks wrapped active filters and cleans up on component dispose', () => {
    const observers = [];
    class ResizeObserverMock {
        constructor(onResize) { this.onResize = onResize; observers.push(this); }
        observe(el) { this.element = el; }
        disconnect() { this.disconnected = true; }
    }
    const context = {
        ResizeObserver: ResizeObserverMock,
        getComputedStyle: () => ({ marginBottom: '8px' }),
    };
    vm.runInNewContext(js.replace('export function ', 'function ') + '\nthis.track = trackFilterHeight;', context);
    const vars = new Map();
    const root = { style: {
        setProperty: (k,v) => vars.set(k,v),
        removeProperty: k => vars.delete(k),
    }};
    let rectHeight=75;
    const filters = { getBoundingClientRect: () => ({height: rectHeight}) };
    const tracker = context.track(root,filters);
    assert.equal(observers.length,1);
    assert.equal(observers[0].element,filters);
    assert.equal(vars.get('--invoice-filters-block-size'),'83px');
    rectHeight=118;
    observers[0].onResize();
    assert.equal(vars.get('--invoice-filters-block-size'),'126px');
    tracker.dispose();
    assert.ok(observers[0].disconnected);
    assert.ok(!vars.has('--invoice-filters-block-size'));
    observers[0].onResize();
    assert.ok(!vars.has('--invoice-filters-block-size'));
});

test('invoice and customer heading/count labels align in one flex row and wrap only when necessary', () => {
    for(const css of [sheet,customerSheet]) {
        assert.match(css,/\.directory-title h1 \{[^}]*display: flex; align-items: center; flex-wrap: wrap; gap: 4px 10px/);
        assert.match(css,/\.directory-count \{[^}]*display: inline-flex; align-items: center; justify-content: center; flex: none; white-space: nowrap; margin: 0;/);
    }
});

test('light Gold uses rich surfaces with dark contrast text independently of darker text accents', () => {
    assert.match(shell,/html\[data-gex-palette="Gold"\]:not\(\[data-gex-mode="dark"\]\)[\s\S]*?background-color: #D1A42D;\s*color: #251D0D;/);
    assert.match(shell,/\.gex-navigation-menu a\.active/);
    assert.match(shell,/\.mud-button-filled-primary/);
});
