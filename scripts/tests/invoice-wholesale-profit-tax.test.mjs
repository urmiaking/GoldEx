import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const from = relative => readFileSync(resolve(root, 'src/App/Client/GoldEx.Client/Pages/Invoices', relative), 'utf8');

test('wholesale product editor never overwrites user-entered profit/tax during parameter updates', () => {
  const source = from('Components/ProductItemEditor.razor.cs');
  const onParametersSet = source.slice(
    source.indexOf('protected override void OnParametersSet()'),
    source.indexOf('protected override async Task OnParametersSetAsync()')
  );
  assert.ok(onParametersSet.includes('base.OnParametersSet()'));
  assert.doesNotMatch(onParametersSet, /Model\.(?:TaxPercent|ProfitPercent)\s*=/);
  assert.doesNotMatch(onParametersSet, /!Model\.Id\.HasValue/);
  const razor = from('Components/ProductItemEditor.razor');
  assert.match(razor, /@bind-Value="Model\.ProfitPercent"/);
  assert.match(razor, /@bind-Value="Model\.TaxPercent"/);
});

test('new wholesale starts with zero while retail sale reads store settings', () => {
  const source = from('Components/EditorForm.razor.cs');
  const start = source.indexOf('private async Task OnAddProductItem()');
  const end = source.indexOf('private async Task OnEditProductItem(', start);
  assert.ok(start >= 0 && end > start);
  const add = source.slice(start, end);
  assert.match(add, /_model\.InvoiceType is InvoiceType\.Sell && _model\.TradeScale is TradeScale\.Retail/);
  assert.match(add, /model\.TaxPercent = isRetailSale \? _setting\?\.TaxPercent \?\? 9 : 0;/);
  assert.match(add, /model\.ProfitPercent = isRetailSale \? _setting\?\.GoldProfitPercent \?\? 7 : 0;/);
  assert.match(add, /\[nameof\(ProductItemEditor\.Model\)\] = model/);
});

test('editing and persistence carry both percentages independently', () => {
  const source = from('ViewModels/ProductItemVm.cs');
  assert.match(source, /_profitPercent = other\.ProfitPercent;/);
  assert.match(source, /_taxPercent = other\.TaxPercent;/);
  assert.match(source, /ProfitPercent = response\.ProfitPercent,/);
  assert.match(source, /TaxPercent = response\.TaxPercent,/);
  assert.match(source, /ProfitPercent = 0,/);
  assert.match(source, /TaxPercent = 0,/);
  assert.match(source, /productItem\.ProfitPercent,/);
  assert.match(source, /productItem\.TaxPercent,/);
});
