import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const read = file => readFileSync(new URL('../../'+file, import.meta.url),'utf8');
const base='src/App/Client/GoldEx.Client/Pages/';
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const dock=read('src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js');
const customerRazor=read(base+'Customers/Components/CustomerFiltersDialog.razor');
const invoiceRazor=read(base+'Invoices/Components/InvoiceFiltersDialog.razor');
const customerCode=read(base+'Customers/Components/CustomerFiltersDialog.razor.cs');
const invoiceCode=read(base+'Invoices/Components/InvoiceFiltersDialog.razor.cs');
const customersList=read(base+'Customers/Components/CustomersList.razor.cs');
const invoicesList=read(base+'Invoices/Components/InvoicesList.razor.cs');
const nav=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor');
const crumb=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/RouteBreadcrumbs.razor');

test('breadcrumb icon is the original SVG chevron pointing left, not a literal arrow',()=>{
 assert.match(crumb,/<ShellIcon Name="chevron-left" \/>/);
 assert.doesNotMatch(crumb,/>←<\/span>/);
 assert.match(css,/\.gex-route-trail-separator \.gex-shell-icon \{/);
});

test('Release Notes is always overflow-only, with exactly one right-aligned proxy',()=>{
 assert.equal((nav.match(/<ReleaseNotesMenuItem\s*\/>/g)||[]).length,1);
 assert.match(dock,/const forceOverflow = link => Boolean\(link\.matches\?\.\('button\.gex-nav-release'\)\)/);
 assert.match(dock,/const routeLinks = links\.filter\(link => !forceOverflow\(link\)\)/);
 assert.match(dock,/const extras = \[\.\.\.routeLinks\.slice\(keep\), \.\.\.links\.filter\(forceOverflow\)\]/);
 assert.match(dock,/String\(extras\.includes\(link\)\)/);
 assert.match(dock,/if \(original\.matches\?\.\('a\[href\]'\)/);
 assert.match(css,/\.gex-liquid-dock \.gex-navigation-menu > button\.gex-nav-release \{ display: none; \}/);
 assert.match(css,/\.gex-liquid-dock \.gex-dock-overflow-links > button\.gex-nav-release \{[\s\S]*?justify-content: flex-start;[\s\S]*?text-align: right;/);
});

test('both filter sheets reuse the responsive customer editor modal and no old MudDialog',()=>{
 for(const source of [customerRazor,invoiceRazor]) {
  assert.match(source,/<EditorDialogFrame>/);
  assert.match(source,/<DialogContent>/);
  assert.match(source,/<DialogActions>/);
  assert.match(source,/<MudGrid Spacing="3">/);
  assert.doesNotMatch(source,/<MudDialog>/);
  assert.match(source,/<MudItem xs="12" Class="gex-filter-mobile-sort">/);
 }
 for(const source of [customerCode, invoiceCode]) {
  assert.match(source,/ResponsiveDialogSession\? Session/);
  assert.match(source,/Session\?\.Close\(result\)/);
  assert.match(source,/Session\?\.Cancel\(\)/);
  assert.doesNotMatch(source,/MudDialog\.Close/);
 }
 assert.match(customersList,/ResponsiveDialogs\.Show<CustomerFiltersDialog>/);
 assert.match(invoicesList,/ResponsiveDialogs\.Show<InvoiceFiltersDialog>/);
 assert.doesNotMatch(customersList,/DialogService\.ShowAsync<CustomerFiltersDialog>/);
 assert.doesNotMatch(invoicesList,/DialogService\.ShowAsync<InvoiceFiltersDialog>/);
});

test('sort controls exist only in mobile filter section while desktop toolbar remains authoritative',()=>{
 assert.match(css,/@media \(min-width: 960px\) \{\s*\.gex-filter-mobile-sort \{ display: none !important; \}/);
 assert.match(customerRazor,/Class="gex-filter-mobile-sort"/);
 assert.match(invoiceRazor,/Class="gex-filter-mobile-sort"/);
 assert.match(customerCode,/keep sorting set by the desktop toolbar/);
 assert.match(invoiceCode,/\[Parameter\] public string SortLabel/);
 assert.match(invoiceCode,/\[Parameter\] public bool SortDescending/);
 assert.match(invoiceCode,/_sortLabel,\s*_sortDescending/);
 assert.match(invoicesList,/nameof\(InvoiceFiltersDialog\.SortLabel\)/);
 assert.match(invoicesList,/nameof\(InvoiceFiltersDialog\.SortDescending\)/);
 assert.match(invoicesList,/_sortDirection = result\.SortDescending/);
});
