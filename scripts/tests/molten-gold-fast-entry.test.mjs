import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
const base = "src/App/Client/GoldEx.Client/Pages/Invoices/";
const markup = readFileSync(base + "MoltenGoldDesk.razor", "utf8");
const viewModel = readFileSync(base + "MoltenGoldDesk.razor.cs", "utf8");
const backend = readFileSync("src/App/Server/GoldEx.Server.Application/Services/MoltenGoldTradeService.cs", "utf8");

test("entry keeps default simple and advanced options optional", () => {
    assert.match(markup, /وزن طلای ۷۵۰/);
    assert.match(markup, /@if \(_advancedOpen\)/);
    assert.match(markup, /@if \(_showImmediate\)/);
    assert.match(markup, /تسویه همزمان \(اختیاری\)/);
});
test("rate basis is submitted and converted with the shared pricing model", () => {
    assert.match(markup, /ValueChanged="ChangeRateBasis"/);
    assert.match(markup, /MoltenGoldRateBasis\.Mesghal/);
    assert.match(viewModel, /MoltenGoldTradePricing\.GramRate\(_rate, _rateBasis/);
    assert.match(viewModel, /_rateBasis, _rate, _commission,/);
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
