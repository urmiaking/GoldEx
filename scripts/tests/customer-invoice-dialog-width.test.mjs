import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const source = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const root = 'src/App/Client/GoldEx.Client.Components/';

test('only invoice customer-history session opts into extra-wide desktop dialog', () => {
    const customers = source('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor.cs');
    const transaction = customers.split('private async Task OnViewTransaction(')[1].split('private async Task OnViewInvoices(')[0];
    const invoices = customers.split('private async Task OnViewInvoices(')[1].split('private async Task RefreshAsync()')[0];
    assert.match(transaction, /wide:\s*true\)/);
    assert.doesNotMatch(transaction, /extraWide/);
    assert.match(invoices, /wide:\s*true,\s*extraWide:\s*true\)/);
});

test('responsive dialog passes optional extra-wide through its session and host', () => {
    const service = source(root + 'Services/ResponsiveDialogService.cs');
    const host = source(root + 'Components/Dialogs/ResponsiveDialogHost.razor');
    const view = source(root + 'Components/Dialogs/ResponsiveDialog.razor');
    const css = source(root + 'Components/Dialogs/ResponsiveDialog.razor.css');
    assert.match(service, /bool extraWide = false/);
    assert.match(service, /public bool ExtraWide \{ get; \}/);
    assert.match(host, /ExtraWide="@\(Top\?\.ExtraWide == true\)"/);
    assert.match(view, /gex-modal-extra-wide/);
    assert.match(css, /\.gex-modal-wide\s*\{[^}]*920px/);
    assert.match(css, /\.gex-modal-extra-wide\s*\{[^}]*1280px/);
    assert.match(css, /@media \(max-width: 640px\)/);
});
