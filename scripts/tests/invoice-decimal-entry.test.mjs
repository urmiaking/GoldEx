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

test('all decimal controls in the item dialog avoid premature 1ms reformatting', () => {
  const fields = [...editor.matchAll(/<MudNumericField\b[\s\S]*?(?:\/>|<\/MudNumericField>)/g)]
    .map(m => m[0])
    .filter(markup => markup.includes('T="decimal"') || markup.includes('T="decimal?"') || markup.includes('Model.GramPrice'));
  assert.ok(fields.length >= 11, 'Expected all decimal product editor inputs');
  for (const field of fields) {
    assert.doesNotMatch(field, /DebounceInterval="1"/);
    assert.match(field, /Immediate="false"/);
  }
  assert.match(editor, /T="int"\s+@bind-Value="Model.Quantity"/);
});

test('gold live-rate input groups thousands during edits while keeping numeric model and validation', () => {
  const field = editor.match(/<MudTextField T="string"[\s\S]*?\/>/)?.[0];
  assert.ok(field, 'Expected live-formatted rate field');
  assert.match(field, /Value="@_gramPriceText"/);
  assert.match(field, /ValueChanged="OnGramPriceTextChanged"/);
  assert.match(field, /Validation=".*ValidateGramPriceText/);
  assert.match(field, /TextUpdateSuppression="false"/);
  assert.match(field, /Immediate="true"/);
  assert.match(field, /InputMode="InputMode.numeric"/);
  assert.match(field, /OnBlur="OnGramPriceBlur"/);
  assert.match(code, /private static string GroupGramPriceDigits\(string\? text\)/);
  assert.match(code, /\(integer.Length - i\) % 3 == 0/);
  assert.match(code, /private static bool TryParseGramPrice\(string\? text, out decimal price\)/);
  assert.match(code, /Model\.GramPrice = price;/);
  assert.match(code, /private static IEnumerable<string> ValidateGramPriceText\(string text\)/);
  assert.match(code, /SyncGramPriceText\(\);/);
  assert.doesNotMatch(field, /DebounceInterval="1"|MudNumericField/);
});