import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const source = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const directoryStyles = [
    ['Customers/Components/CustomersList.razor.css', false],
    ['Invoices/Components/InvoicesList.razor.css', true],
    ['InventoryStocks/Components/InventoryDirectory.razor.css', true],
    ['InventoryStocks/Components/MeltingBatchesList.razor.css', true],
    ['Finances/Checks/Components/CheckPaymentsList.razor.css', true],
    ['Finances/PaymentVouchers/Components/PaymentVouchersList.razor.css', true],
    ['Finances/CustomerTransfers/Components/CustomerTransferList.razor.css', true],
];

test('directory sticky offset includes the 46px mobile breadcrumb and preserves desktop offset', () => {
    const css = source('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
    assert.match(css, /--gex-directory-sticky-top:\s*calc\(var\(--gex-appbar-height\) \+ env\(safe-area-inset-top, 0px\) \+ 8px\)/);
    assert.match(css, /@media \(max-width: 959\.98px\) \{\s*\.goldex-shell \{\s*--gex-mobile-crumb-height: 46px;[\s\S]*?--gex-directory-sticky-top:\s*calc\(var\(--gex-appbar-height\) \+ var\(--gex-mobile-crumb-height\) \+ env\(safe-area-inset-top, 0px\) \+ 8px\)/);
    assert.match(css, /top: calc\(var\(--gex-appbar-height\) \+ env\(safe-area-inset-top, 0px\)\);\s*inset-inline: 0; height: var\(--gex-mobile-crumb-height\)/);
});

test('all seven directory filter bars and their desktop sticky table headers share one offset', () => {
    const prefix = 'src/App/Client/GoldEx.Client/Pages/';
    for (const [path, hasStickyHeader] of directoryStyles) {
        const css = source(prefix + path);
        assert.equal((css.match(/top:\s*var\(--gex-directory-sticky-top\)/g) ?? []).length, 1, path);
        const headers = css.match(/top:\s*calc\(var\(--gex-directory-sticky-top\) \+ var\(--invoice-filters-block-size,\s*96px\)\)/g) ?? [];
        assert.equal(headers.length, hasStickyHeader ? 1 : 0, path);
        assert.doesNotMatch(css, /top:\s*calc\(var\(--gex-appbar-height,\s*70px\)/, path);
    }
});

test('customer invoice dialog retains a local sticky coordinate system', () => {
    const css = source('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
    assert.match(css, /\.invoice-directory\.in-customer-dialog \.directory-filters \{ top: 0; \}/);
    assert.match(css, /\.invoice-directory\.in-customer-dialog \.invoice-table th \{\s*top: var\(--invoice-filters-block-size, 96px\)/);
});