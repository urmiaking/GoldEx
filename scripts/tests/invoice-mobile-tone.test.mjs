import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const read = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const invoices = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const code = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
const css = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
const appearance = read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const customer = read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerCard.razor');

test('mobile invoice trade chips have consistent sizing and paid indicator is compact', () => {
    assert.match(css, /\.invoice-card-head \.invoice-badge\s*\{[^}]*min-height: 26px;[^}]*font-size: \.8125rem;[^}]*font-weight: 500/);
    assert.match(css, /\.invoice-card-amounts \.invoice-paid\s*\{[^}]*min-height: 28px;[^}]*font-size: \.75rem;[^}]*font-weight: 500/);
    assert.match(css, /\.invoice-card-amounts \.invoice-paid \.gex-shell-icon\s*\{\s*width: 14px; height: 14px;/);
    assert.match(invoices, /class="invoice-badge @\(invoice\.InvoiceType == InvoiceType\.Sell/);
    assert.match(invoices, /class="invoice-badge is-scale"/);
    assert.equal(invoices.split('class="invoice-paid"').length - 1, 2);
});

test('semantic invoice card glow is limited to unsettled balance states', () => {
    assert.match(invoices, /class="invoice-card @UnpaidClass\(invoice\)/);
    assert.match(code, /if \(invoice.PaymentStatus == InvoicePaymentStatus.Paid\) return "is-paid";/);
    assert.match(code, /if \(invoice.PaymentStatus == InvoicePaymentStatus.Overdue\) return "is-overdue";/);
    assert.match(code, /return color == Color.Success \? "is-credit" : color == Color.Error \? "is-debit" : "";/);
    assert.match(css, /\.invoice-card:is\(\.is-credit, \.is-debit, \.is-overdue\)\s*\{/);
    assert.match(css, /--invoice-state: var\(--mud-palette-success\)/);
    assert.match(css, /\.invoice-card:is\(\.is-debit, \.is-overdue\)\s*\{\s*--invoice-state: var\(--mud-palette-error\)/);
    assert.match(css, /border-inline-start: 3px solid color-mix/);
    assert.match(css, /color-mix\(in srgb, var\(--invoice-state\) 5%, transparent\)/);
    assert.doesNotMatch(css, /\.invoice-card:is\([^)]*\.is-paid/);
    assert.match(customer, /<CustomerRemaining/); // Async multi-currency balance: do not infer overall status from one.
});

test('standard 500 weights for desktop sidebar create and active navigation including mobile', () => {
    assert.match(appearance, /\.gex-sidebar-create\s*\{[^}]*font-weight: 500/);
    assert.match(appearance, /\.gex-navigation-menu a.active\s*\{[^}]*font-weight: 500/);
    assert.match(appearance, /\.gex-bottom-nav > a.active\s*\{[^}]*font-weight: 500/);
    assert.match(appearance, /\.gex-bottom-nav \.gex-bottom-create\s*\{[^}]*font-weight: 500/);
    assert.match(appearance, /html\[data-gex-palette="Gold"\]:not\(\[data-gex-mode="dark"\]\)[\s\S]*?\.gex-navigation-menu a\.active/);
});

test('invoice new menu aligns to the filter card at desktop only', () => {
    assert.match(css, /@media \(min-width: 960px\)\s*\{[^}]*\.directory-heading ::deep \.directory-create\s*\{\s*margin-inline-end: -16px;/);
    assert.match(invoices, /Class="directory-create"/);
    assert.match(css, /\.directory-toolbar \{[^}]*padding: 14px 16px/);
    assert.match(css, /@media \(max-width: 699px\)/);
});
