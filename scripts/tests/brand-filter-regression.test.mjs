import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const bar=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const appCss=read('src/App/Server/GoldEx.Server/wwwroot/css/app.css');
const report=read('src/App/Client/GoldEx.Client/Pages/Reporting/Components/ReportFiltersBase.razor');
const invoice=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor');
const customer=read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerFiltersDialog.razor');

test('center GoldEx text reuses the exact original Cinzel Decorative gold-gradient brand',()=>{
 assert.match(bar, /class="gex-desktop-wordmark brand-text"/);
 assert.match(appCss, /\.brand-text \{[\s\S]*?font-family: "Cinzel Decorative", "Outfit", "Inter", serif;/);
 assert.match(appCss, /\.brand-text \{[\s\S]*?linear-gradient\(90deg, #b8860b 0%, #daa520 25%, #ffd700 50%/);
 assert.match(appCss, /\.brand-text \{[\s\S]*?-webkit-text-fill-color: transparent/);
 assert.match(css,/\.goldex-shell \.gex-desktop-wordmark\.brand-text \{[\s\S]*?font-family:"Cinzel Decorative","Outfit","Inter",serif;/);
 assert.match(css,/\.goldex-shell \.gex-desktop-wordmark\.brand-text \{[\s\S]*?font-weight:800; margin-top:0/);
 assert.match(css,/\.goldex-shell \.gex-desktop-wordmark\.brand-text \{[\s\S]*?left:50%;[\s\S]*?transform:translate\(-50%,-50%\)/);
});
test('invoice and customer filter dates retain compact mobile and two-column desktop grid',()=>{
 for(const source of [invoice,customer]){
  assert.match(source,/<MudItem xs="12" sm="6">\s*<MudDateRangePicker/);
  assert.match(source,/DateRangeChanged="@\(dr => _dateRange = dr\)"/);
  assert.match(source,/MaxMonthColumns="1"/);
  assert.match(source,/DisplayMonths="1"/);
  assert.match(source,/Rounded="true"/);
  assert.match(source,/AnchorOrigin="Origin.BottomCenter"/);
  assert.match(source,/TransformOrigin="Origin.TopCenter"/);
  assert.match(source,/RelativeWidth="DropdownWidth.Adaptive"/);
  assert.match(source,/Adornment="Adornment.Start"/);
  assert.match(source,/Class="w-100"/);
  assert.doesNotMatch(source,/gex-desktop-filter-date/);
  assert.match(source,/<EditorDialogFrame>/);
 }
 assert.match(report,/RelativeWidth="DropdownWidth.Adaptive"/);
});
test('never wrap MudDateRangePicker internals in padded, double-outlined desktop CSS',()=>{
 assert.doesNotMatch(css,/\.gex-directory-filter-form \.gex-desktop-filter-date/);
 assert.doesNotMatch(css,/\.gex-desktop-filter-date \.mud-input-root/);
 assert.doesNotMatch(css,/\.gex-desktop-filter-date \{/);
});
