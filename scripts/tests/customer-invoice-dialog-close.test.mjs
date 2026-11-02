import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';

const read = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');

test('closing the customer invoice viewer does not reload customers', () => {
    const code = read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor.cs');
    const invoiceView = code.split('private async Task OnViewInvoices(')[1].split('private async Task RefreshAsync()')[0];
    assert.match(invoiceView, /await dialog\.Result;/);
    assert.doesNotMatch(invoiceView, /RefreshAsync\(|LoadCustomersAsync\(/);
    const edit = code.split('private async Task OnEdit(')[1].split('private async Task OnViewTransaction(')[0];
    assert.match(edit, /await RefreshAsync\(\);/);
});

test('desktop native dialog clips its contents to rounded corners', () => {
    const css = read('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor.css');
    const panel = css.match(/^\.gex-modal-panel\s*\{([^}]*)\}/m)?.[1];
    assert.ok(panel, 'shared modal panel CSS must exist');
    assert.match(panel, /border-radius:\s*20px/);
    assert.match(panel, /overflow:\s*hidden/);
    assert.match(css, /@media \(max-width: 640px\)/);
    assert.match(css, /border-radius:\s*20px 20px 12px 12px/);
});
