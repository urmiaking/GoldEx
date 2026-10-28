import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const load = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const shell = load('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const invoice = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const css = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');

function luminance(hex) {
    const values = [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255)
        .map(v => v <= 0.04045 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4);
    return values[0] * 0.2126 + values[1] * 0.7152 + values[2] * 0.0722;
}
function contrast(a, b) {
    const high = Math.max(luminance(a), luminance(b));
    const low = Math.min(luminance(a), luminance(b));
    return (high + 0.05) / (low + 0.05);
}

test('white text and icons on selected Gold menu and filled gold controls maintain AA contrast', () => {
    const activeBlock = shell.match(/html\[data-gex-palette="Gold"\]:not\(\[data-gex-mode="dark"\]\) \.goldex-shell :is\([\s\S]*?\):not\(:disabled\):not\(\.mud-disabled\) \{\s*background-color: (#[A-Fa-f0-9]{6});\s*color: (#[A-Fa-f0-9]{6});/);
    assert.ok(activeBlock, 'Light Gold filled controls and active menu styles must exist');
    assert.ok(contrast(activeBlock[1], activeBlock[2]) >= 4.5);
    assert.equal(activeBlock[2].toUpperCase(), '#FFFFFF');
    for (const control of ['.gex-navigation-menu a.active', '.gex-settings-navigation-section a.active',
        '.mud-button-filled-primary', '.mud-button-filled-secondary', '.gex-create-circle']) {
        assert.ok(activeBlock[0].includes(control), 'Missing colored control ' + control);
    }
    assert.match(shell, /:is\(\.mud-icon-root, \.gex-shell-icon\) \{\s*color: #FFFFFF;/);
    assert.match(shell, /background-color: #9D6800;\s*color: #FFFFFF;/);
    assert.ok(contrast('#9D6800', '#FFFFFF') >= 4.5);
});

test('desktop and mobile invoice customer phones follow names at the RTL right edge while keeping LTR digits', () => {
    const tableStart = invoice.indexOf('<table class="invoice-table');
    const desktop = invoice.slice(tableStart, invoice.indexOf('</table>', tableStart));
    const mobile = invoice.slice(invoice.indexOf('<div class="invoice-mobile-list">'));
    for (const block of [desktop, mobile]) {
        assert.match(block, /@invoice\.CustomerFullName[\s\S]*?<small class="invoice-customer-phone" dir="ltr">@invoice\.CustomerPhoneNumber<\/small>/);
    }
    assert.equal(invoice.split('class="invoice-customer-phone" dir="ltr"').length - 1, 2);
    assert.match(css, /\.invoice-customer \{\s*justify-items: start; text-align: right;\s*\}/);
    assert.match(css, /\.invoice-customer-phone \{[^}]*direction: ltr;[^}]*text-align: right;[^}]*white-space: nowrap;/);
    assert.match(css, /\.invoice-card-customer > div \{[^}]*justify-items: start; text-align: right;/);
});
