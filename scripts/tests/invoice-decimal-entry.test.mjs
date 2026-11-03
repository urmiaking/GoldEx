import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const editor = readFileSync(resolve(root, 'src/App/Client/GoldEx.Client/Pages/Invoices/Components/ProductItemEditor.razor'), 'utf8');
const code = readFileSync(resolve(root, 'src/App/Client/GoldEx.Client/Pages/Invoices/Components/ProductItemEditor.razor.cs'), 'utf8');

test('weight, profit and tax preserve multi-digit and fractional input until Enter or blur', () => {
  for (const field of ['Model.TotalWeight', 'Model.ProfitPercent', 'Model.TaxPercent']) {
    const component = [...editor.matchAll(/<MudNumericField\b[\s\S]*?\/>/g)]
      .map(match => match[0])
      .find(markup => markup.includes(`For="() => ${field}"`) || markup.includes(`For="@(() => ${field})"`));
    assert.ok(component, `Missing numeric field ${field}`);
    assert.match(component, /Immediate="false"/);
    assert.doesNotMatch(component, /DebounceInterval="1"/);
    assert.match(component, /Format="G29"/);
  }
  assert.match(editor, /ValueChanged="OnTotalWeightChanged"/);
  assert.match(code, /Model\.TotalWeight = totalWeight;/);
  assert.match(editor, /@bind-Value="Model.ProfitPercent"/);
  assert.match(editor, /@bind-Value="Model.TaxPercent"/);
});

test('fractional numeric fields retain their non-immediate behavior and have no 1ms debounce', () => {
  const fields = [...editor.matchAll(/<MudNumericField\b[\s\S]*?(?:\/>|<\/MudNumericField>)/g)]
    .map(m => m[0])
    .filter(markup => markup.includes('T="decimal"') || markup.includes('T="decimal?"'));
  assert.ok(fields.length >= 11, 'Expected the decimal item editor controls');
  for (const field of fields) {
    assert.doesNotMatch(field, /DebounceInterval="1"/);
    assert.match(field, /Immediate="false"/);
  }
  assert.match(editor, /T="int"\s+@bind-Value="Model.Quantity"/);
});

test('gram price uses the same MudNumericField format and immediate behavior as calculator', () => {
  const calculator = readFileSync(resolve(root,
    'src/Calculator/Client/GoldEx.Calculator.Client/Components/TradeInCalculatorComponent.razor'), 'utf8');
  const gram = [...editor.matchAll(/<MudNumericField\b[\s\S]*?\/>/g)]
    .map(m => m[0])
    .find(markup => markup.includes('@bind-Value="Model.GramPrice"'));
  const calcRate = [...calculator.matchAll(/<MudNumericField\b[\s\S]*?\/>/g)]
    .map(m => m[0])
    .find(markup => markup.includes('@bind-Value="_model.NewGramPrice"'));
  assert.ok(gram, 'Gram price must retain a numeric model binding');
  assert.ok(calcRate, 'Calculator gold rate reference is missing');
  for (const markup of [gram, calcRate]) {
    assert.match(markup, /Format="#,##0\.##"/);
    assert.match(markup, /Immediate="true"/);
    assert.match(markup, /HideSpinButtons="true"/);
  }
  assert.doesNotMatch(gram, /DebounceInterval="1"/);
  assert.doesNotMatch(editor, /_gramPriceText|GroupGramPriceDigits|OnGramPriceTextChanged/);
  assert.doesNotMatch(code, /_gramPriceText|GroupGramPriceDigits|SyncGramPriceText/);
  assert.match(editor, /For="@\(\(\) => Model\.GramPrice\)"/);
});
