import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const base = 'src/App/Client/GoldEx.Client/Pages/Invoices/';
const invoicesCss = readFileSync(base + 'Components/InvoicesList.razor.css', 'utf8');
const desk = readFileSync(base + 'MoltenGoldDesk.razor', 'utf8');
const deskCss = readFileSync(base + 'MoltenGoldDesk.razor.css', 'utf8');

test('mobile invoice create MudMenu has no extra wrapper padding', () => {
    assert.match(invoicesCss, /@media \(max-width: 959px\)[\s\S]*?\.directory-heading ::deep \.directory-create \{ padding: 0; \}/);
    assert.match(invoicesCss, /@media \(max-width: 699px\)[\s\S]*?\.directory-heading ::deep \.directory-create \{[^}]*padding: 0;/);
    assert.match(invoicesCss, /\.directory-create \.mud-button-root \{[^}]*padding: 7px 12px;/);
});

test('molten trade labels and submit action are short and consistent', () => {
    assert.match(desk, /می‌خرم/);
    assert.match(desk, /می‌فروشم/);
    assert.match(desk, /Class="desk-create-btn">\s*ثبت معامله/);
    assert.doesNotMatch(desk, /خرید از طرف‌حساب|فروش به طرف‌حساب/);
});

test('settlement toggles do not grow beyond mobile panel', () => {
    assert.match(deskCss, /\.desk-options-toggle \{[^}]*box-sizing:border-box;/);
    assert.match(deskCss, /\.desk-options-toggle > span:first-child \{[^}]*white-space:normal;/);
    assert.match(deskCss, /\.desk-options-toggle > span:nth-child\(2\) \{[^}]*min-width:0;/);
    assert.match(deskCss, /\.desk-immediate-section \{[^}]*grid-template-columns:minmax\(0,1fr\)/);
});