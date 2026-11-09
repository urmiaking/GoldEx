import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
const base = "src/App/Client/GoldEx.Client/Pages/Invoices/";
const markup = readFileSync(base + "MoltenGoldDesk.razor", "utf8");
const viewModel = readFileSync(base + "MoltenGoldDesk.razor.cs", "utf8");
const backend = readFileSync("src/App/Server/GoldEx.Server.Application/Services/MoltenGoldTradeService.cs", "utf8");

test("quick entry uses 18K gram price only, never mesghal selector", () => {
    assert.match(markup, /Label="نرخ هر گرم طلای ۱۸ عیار"/);
    assert.doesNotMatch(markup, /مبنای نرخ|مظنه \(مثقال|MudSelect T="MoltenGoldRateBasis"/);
    assert.match(viewModel, /MoltenGoldRateBasis\.Gram, _rate, _commission/);
    assert.doesNotMatch(viewModel, /ChangeRateBasis|GramRate =>|_rateBasis/);
});
test("loading keeps buy and sell visible and skeletonizes the ledger table", () => {
    assert.match(markup, /Class="desk-buy"/);
    assert.match(markup, /Class="desk-sell"/);
    assert.match(markup, /@if \(_loading \|\| _isInitializing\)/);
    assert.match(markup, /else if \(_settings is null\)/);
    assert.match(markup, /desk-table-loading/);
    assert.match(markup, /@for \(var i = 0; i < 5; i\+\+\)/);
    assert.doesNotMatch(markup, /در حال دریافت تنظیمات دفتر آب‌شده/);
});
test("rate freshness is in form heading, outside the inputs", () => {
    assert.match(markup, /class="desk-rate-updated"/);
    assert.ok(markup.indexOf("desk-rate-updated") < markup.indexOf("desk-primary-fields"));
    assert.doesNotMatch(markup, /desk-rate-note/);
});
test("advanced fields and immediate settlement animate closed and opened", () => {
    const css = readFileSync(base + "MoltenGoldDesk.razor.css", "utf8");
    assert.equal((markup.match(/class="@\(_\w+ \? "desk-expand is-open"/g) || []).length, 2);
    assert.match(markup, /inert="@\(!_advancedOpen\)"/);
    assert.match(markup, /inert="@\(!_showImmediate\)"/);
    assert.match(css, /grid-template-rows:\s*0fr/);
    assert.match(css, /grid-template-rows:\s*1fr/);
    assert.match(css, /prefers-reduced-motion:reduce/);
});
test("balances use exact configured account units", () => {
    assert.match(viewModel, /b\.PriceUnit\.Id == _settings!\.GoldPriceUnitId/);
    assert.match(viewModel, /b\.PriceUnit\.Id == _settings!\.PriceUnitId/);
    assert.doesNotMatch(viewModel, /Contains\("مثقال"\)/);
});
test("unsettled trades include remaining physical gold", () => {
    assert.match(backend, /state\.Remaining <= 0 && trade\.RemainingWeight <= 0/);
});
test("commission differs by trade direction", () => {
    assert.match(viewModel, /_side == InvoiceType\.Sell \? _settings\.CommissionPercent : 0m/);
});

test("routine trade actions stay visible and advanced ones stay collapsed", () => {
    const details = readFileSync(base + "Components/MoltenGoldTradeDetails.razor", "utf8");
    assert.match(details, /trade-actions-bar/);
    assert.match(details, /<details class="trade-secondary-actions">/);
    assert.match(details, /MoltenGoldOperation\.GoldSettlement/);
});

test("selected immediate payments remain visible when collapsed", () => {
    assert.match(markup, /ImmediateSummary/);
    assert.match(viewModel, /if \(_immediatePayment is not null\)/);
    assert.match(viewModel, /if \(_immediateDelivery is not null\)/);
});
