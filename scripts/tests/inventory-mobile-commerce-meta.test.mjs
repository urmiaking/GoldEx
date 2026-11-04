import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const source = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const markup = source('src/App/Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectory.razor');
const css = source('src/App/Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectory.razor.css');
const code = source('src/App/Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectory.razor.cs');
const mobile = markup.slice(markup.indexOf('<div class="stock-mobile-list">'));

test('mobile product cards show the same fixed, percentage, and sold wage as desktop', () => {
    assert.match(mobile, /else if \(item\.Product is not null\)\s*\{\s*<div class="stock-card-meta stock-card-product-meta">/);
    assert.match(mobile, /<span class="stock-card-wage">@GetWageDisplay\(item\)<\/span>/);
    assert.match(code, /ItemStatus == ItemStatus\.Sold \? item\.SaleWage : item\.Product\?\.Wage/);
    assert.match(code, /WageType\.Fixed =>/);
    assert.match(code, /WageType\.Percent =>/);
    assert.ok(mobile.indexOf('ItemType == ItemType.MoltenGold') < mobile.indexOf('stock-card-product-meta'));
    assert.ok(mobile.indexOf('stock-card-product-meta') < mobile.indexOf('else if (item.Coin is not null)'));
});

test('vitrine badge shows both states only for eligible gold and jewelry products', () => {
    assert.match(mobile, /@if \(CanShowVitrine\(item\)\)/);
    assert.match(mobile, /item\.Product\.ShowInVitrine \? "is-visible" : "is-hidden"/);
    assert.match(mobile, /item\.Product\.ShowInVitrine \? "در ویترین" : "خارج از ویترین"/);
    assert.match(code, /CanShowVitrine\(InventoryStockVm item\) =>\s*item\.Product\?\.ProductType is ProductType\.Gold or ProductType\.Jewelry/);
    assert.match(mobile, /else if \(item\.Coin is not null\)/);
    assert.match(mobile, /<div class="stock-card-amounts">/);
});

test('vitrine chip is mobile-only, compact, wrapping, and theme-aware', () => {
    assert.match(css, /@media \(max-width:959\.98px\) \{[\s\S]*?\.stock-card-product-meta \{[^}]*gap:6px 9px/);
    assert.match(css, /\.stock-card-product-meta \.stock-card-wage \{[^}]*overflow-wrap:anywhere/);
    assert.match(css, /\.stock-card-product-meta \.stock-vitrine-chip \{[^}]*border-radius:999px;[^}]*font-size:\.72rem/);
    assert.match(css, /\.stock-card-product-meta \.stock-vitrine-chip\.is-visible \{[^}]*var\(--mud-palette-success\)/);
    assert.doesNotMatch(markup.slice(0,markup.indexOf('<div class="stock-mobile-list">')), /stock-vitrine-chip/);
});