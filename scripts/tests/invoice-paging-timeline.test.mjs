import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const load = p => readFileSync(new URL('../../' + p, import.meta.url), 'utf8');
const invoices = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const mainCode = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.cs');
const mainCss = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
const detail = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor');
const detailCss = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor.css');
const typeEnum = load('src/App/Shared/GoldEx.Shared/Enums/TransactionType.cs');
const warehouseEnum = load('src/App/Shared/GoldEx.Shared/Enums/WarehouseActionType.cs');

test('paging immediately replaces previous invoice rows with desktop and mobile skeletons', () => {
    assert.match(invoices, /@if \(_loading\)\s*\{\s*<div class="invoice-placeholder"/);
    assert.ok(invoices.indexOf('@if (_loading)') < invoices.indexOf('else if (_loadFailed)'));
    assert.match(invoices, /class="invoice-placeholder-desktop"/);
    assert.match(invoices, /<table class="invoice-skeleton-table"/);
    assert.match(invoices, /class="invoice-placeholder-mobile"/);
    assert.match(invoices, /class="invoice-skeleton-card"/);
    assert.match(invoices, /aria-busy="@_loading"/);
    assert.doesNotMatch(invoices, /invoice-refreshing/);
    assert.match(mainCode, /_loading = true;[\s\S]*?await InvokeAsync\(StateHasChanged\);[\s\S]*?await Task.Yield\(\);[\s\S]*?service\.GetListAsync/);
    assert.match(mainCss, /\.invoice-placeholder-mobile \{ display: none; \}/);
    assert.match(mainCss, /@media \(max-width: 959\.98px\)[\s\S]*?\.invoice-placeholder-mobile \{ display: grid/);
});

test('invoice pager has rows on RTL right, previous/current/next on left with 10 default', () => {
    const footer = invoices.slice(invoices.indexOf('<footer class="invoice-pagination"'));
    assert.ok(footer.indexOf('class="invoice-page-size"') < footer.indexOf('class="invoice-pagination-actions"'));
    assert.match(footer, /<nav class="invoice-pagination-actions" aria-label="جابجایی بین صفحه‌ها">/);
    assert.match(footer, /class="invoice-pagination-current"/);
    assert.deepEqual([...footer.matchAll(/<option value="(\d+)"/g)].map(x => Number(x[1])), [10,25,50]);
    assert.match(mainCode, /_pageSize = 10/);
    assert.match(mainCode, /SizeQuery is 10 or 25 or 50 \? SizeQuery\.Value : 10/);
    assert.match(mainCode, /size is not \(10 or 25 or 50\)/);
    assert.match(mainCode, /if \(_pageSize != 10\)/);
    assert.match(mainCss, /\.invoice-pagination \{[^}]*justify-content: space-between/);
    assert.match(mainCss, /\.invoice-pagination-actions \{[^}]*margin-inline-start: auto/);
    assert.doesNotMatch(footer, /نمایش .* تا .* از/);
});

test('paid invoices show the green status chip in both table and mobile cards', () => {
    assert.equal(invoices.split('class="invoice-paid" aria-label="تسویه حساب شده"').length - 1, 2);
    assert.match(mainCss, /\.invoice-paid \{[^}]*background: var\(--mud-palette-success\)/);
    assert.match(mainCss, /\.invoice-paid \{[^}]*color: var\(--mud-palette-success-text/);
});

test('financial details are a timeline with distinct credit green and debit red chips', () => {
    assert.match(typeEnum, /Credit = 0/);
    assert.match(typeEnum, /Debit = 1/);
    assert.equal(detail.split('<ol class="invoice-event-timeline"').length - 1, 2);
    assert.doesNotMatch(detail, /invoice-details-table|MudTable|MudChip/);
    assert.match(detail, /item.TransactionType == TransactionType.Credit \? "is-credit" : "is-debit"/);
    assert.match(detail, /@item.TransactionType.GetDisplayName\(\)/);
    assert.match(detail, /@item.Amount.ToCurrencyFormat\(item.PriceUnit\)/);
    assert.match(detail, /@item.Description/);
    assert.match(detail, /@item.LedgerAccount/);
    assert.match(detailCss, /\.invoice-event-type\.is-credit \{[^}]*background: var\(--mud-palette-success\)/);
    assert.match(detailCss, /\.invoice-event-type\.is-debit \{[^}]*background: var\(--mud-palette-error\)/);
    assert.match(detailCss, /\.invoice-event-amount\.is-credit \{[^}]*color: var\(--mud-palette-success\)/);
    assert.match(detailCss, /\.invoice-event-amount\.is-debit \{[^}]*color: var\(--mud-palette-error\)/);
});

test('inventory events, financial balances and independent detail paging are preserved', () => {
    assert.match(warehouseEnum, /In = 0/);
    assert.match(warehouseEnum, /Out = 1/);
    assert.match(detail, /item.ActionType == WarehouseActionType.In \? "is-credit" : "is-debit"/);
    assert.match(detail, /WarehouseActionType.In \? "ورود به انبار" : "خروج از انبار"/);
    assert.match(detail, /ToWeightFormat/);
    assert.match(detail, /ToCurrencyFormat/);
    assert.match(detail, /GetInvoiceInventoryItemsAsync/);
    assert.match(detail, /ITransactionService/);
    assert.match(detail, /IInventoryStockService/);
    assert.match(detail, /ChangeTransactionPage/);
    assert.match(detail, /ChangeInventoryPage/);
    assert.equal(detail.split('<CustomerRemaining ').length - 1, 2);
    assert.match(detail, /private readonly string _panelId =/);
    assert.match(detail, /aria-controls="@\(\$"\{_panelId\}-transactions"\)"/);
    assert.equal(detail.split('class="invoice-event-loading" role="status"').length - 1, 2);
    assert.match(detail, /class="invoice-event-skeleton"/);
    assert.match(detailCss, /@media \(prefers-reduced-motion: reduce\)[\s\S]*invoice-event-skeleton > span \{ animation: none;/);    assert.match(detailCss, /@media \(max-width: 680px\)/);
    assert.match(detailCss, /@media \(prefers-reduced-motion: reduce\)/);
});
