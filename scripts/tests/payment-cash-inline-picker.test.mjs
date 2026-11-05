import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const read = name => readFileSync(new URL('../../' + name, import.meta.url), 'utf8');
const markup = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/PaymentEditor.razor');
const logic = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/PaymentEditor.razor.cs');
const css = read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/PaymentEditor.razor.css');
const shared = read('src/App/Client/GoldEx.Client.Components/wwwroot/js/responsive-dialog.js');
const cash = markup.slice(markup.indexOf('case PaymentType.InternalCash:'), markup.indexOf('case PaymentType.CustomerTransfer:'));

test('cash account dropdown floats over the dialog without reserving layout space', () => {
  assert.match(cash, /<MudTextField T="string"/);
  assert.match(cash, /ValueChanged="OnCashAccountSearchChanged"/);
  assert.match(cash, /class="cash-account-options" role="listbox"/);
  assert.match(cash, /@onpointerdown="\(\) => ChooseCashAccount\(account\)"/);
  assert.doesNotMatch(cash, /<MudAutocomplete/);
  assert.match(css, /\.cash-account-field-row \{[^}]*position:relative;[^}]*z-index:5/);
  assert.match(css, /\.cash-account-picker \{[^}]*position:relative/);
  assert.match(css, /\.cash-account-options \{[^}]*position:absolute;[^}]*top:calc\(100% \+ 4px\);[^}]*inset-inline:0/);
  assert.match(css, /\.cash-account-options \{[^}]*overflow-y:auto/);
  assert.doesNotMatch(css, /\.cash-account-options[^}]*position:fixed/);
});
test('typed account search filters and selection updates the financial account model', () => {
  assert.match(logic, /account\.Title\.Contains\(_cashAccountSearch, StringComparison\.InvariantCultureIgnoreCase\)/);
  assert.match(logic, /Model\.FinancialAccount = account;/);
  assert.match(logic, /Model\.FinancialAccount = null;/);
  assert.match(logic, /_cashAccountSearch = account\.Title;/);
  assert.match(cash, /@onpointerdown:preventDefault="true"/);
  assert.match(logic, /case "Enter" when accounts\.Length > 0:/);
  assert.match(logic, /case "Escape":/);
});
test('required account validation and account creation still work', () => {
  assert.match(logic, /_cashAccountInvalid = Model\.PaymentType is PaymentType\.InternalCash/);
  assert.match(logic, /!_form\.IsValid \|\| _cashAccountInvalid/);
  assert.match(cash, /role="alert">وارد کردن حساب مالی الزامی است/);
  assert.match(logic, /Model\.FinancialAccount = Model\.FinancialAccounts\?\.FirstOrDefault/);
  assert.match(cash, /OnClick="OnAddFinancialAccount"/);
});
test('unrelated select dialog behavior restored to the previous shared implementation', () => {
  assert.match(shared, /for \(const popup of overlay\.querySelectorAll\('\.mud-popover-open\[id\^="popovercontent-"\]'\)\)/);
  assert.doesNotMatch(shared, /popup\.style\.setProperty\(name, value, 'important'\)/);
});