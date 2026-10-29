import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const read = path => readFileSync(new URL('../../'+path,import.meta.url),'utf8');
const components='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const bar=read(components+'AppBar.razor');
const identity=read(components+'ShellIdentity.razor');
const nav=read(components+'NavMenu.razor');
const drawer=read(components+'Drawer.razor');
const license=read(components+'DesktopLicenseRibbon.razor');
const customer=read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerFiltersDialog.razor');
const invoice=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor');
const modalCss=read('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor.css');

test('desktop owns the redesigned centered brand and body-aligned route trail',()=>{
 assert.match(bar,/<div class="gex-desktop-brand"><ShellBrand Compact="true" \/><\/div>/);
 assert.match(bar,/<\/header>\s*<div class="gex-desktop-route-bar"><RouteBreadcrumbs \/><\/div>/);
 assert.match(css,/@media \(min-width: 960px\)[\s\S]*?\.goldex-shell \.gex-desktop-brand \{[\s\S]*?left: 50%;[\s\S]*?transform: translate\(-50%, -50%\)/);
 assert.match(css,/\.gex-desktop-route-bar \{[\s\S]*?inset-inline-start: calc\(var\(--gex-dock-size\) \+ 44px\)/);
 assert.match(css,/--gex-content-offset: calc\(var\(--gex-appbar-height\)[^;]*var\(--gex-desktop-crumb-height\)/);
 assert.match(css,/\.goldex-shell \.gex-topbar-actions \{[\s\S]*?margin-inline-end: auto/);
 assert.match(bar,/<Clock \/>/);
 assert.match(bar,/<AppearanceControls ModeOnly="true" \/>/);
 assert.match(bar,/<AuthorizeView><Authorized><Settings \/><\/Authorized><\/AuthorizeView>/);
});
test('mobile header and date filters are back to original compact behavior',()=>{
 assert.match(bar,/<div class="gex-mobile-brand"><ShellBrand Compact="true" \/><\/div>/);
 assert.match(bar,/gex-mobile-route-bar.*RouteBreadcrumbs/);
 assert.doesNotMatch(css,/\/\* #263: Approved premium mobile shell/);
 assert.doesNotMatch(nav,/gex-mobile-more-heading/);
 assert.match(nav,/<ReleaseNotesMenuItem \/>/);
 assert.doesNotMatch(identity,/MobileLicenseRibbon|gex-mobile-license/);
 assert.match(css,/@media \(max-width: 959\.98px\)[\s\S]*?\.gex-mobile-brand, \.gex-menu-toggle \{ display: flex;/);
 for (const source of [invoice,customer]) {
  assert.match(source,/<MudDateRangePicker DateRange="@_dateRange"/);
  assert.match(source,/Adornment="Adornment.Start"/);
  assert.match(source,/Class="w-100 gex-desktop-filter-date"/);
  assert.doesNotMatch(source,/gex-filter-date-card|gex-directory-filter-actions|PlaceholderStart="از تاریخ"/);
 }
 assert.match(css,/@media \(min-width: 960px\) \{[\s\S]*?\.gex-directory-filter-form \.gex-desktop-filter-date/);
 assert.match(css,/@media \(max-width: 959\.98px\) \{[\s\S]*?\.gex-desktop-route-bar, \.gex-desktop-profile-license \{ display: none !important;/);
});
test('license expiry and renewal now belong to desktop profile rather than More',()=>{
 assert.match(identity,/<div class="gex-desktop-profile-license"><DesktopLicenseRibbon \/><\/div>/);
 assert.match(license,/LicenseState\.Current/);
 assert.match(license,/LicenseState\.OnChange \+= OnLicenseChanged/);
 assert.match(license,/LicenseState\.OnChange -= OnLicenseChanged/);
 assert.match(license,/ExpireDate\.Date/);
 assert.match(license,/ClientRoutes.RegisterProduct.Extend/);
 assert.match(license,/ClientRoutes.RegisterProduct.Upgrade/);
 assert.doesNotMatch(drawer,/<LicenseStatusBanner/);
 assert.match(css,/\.gex-liquid-dock \.gex-sidebar-identity:is\(:hover,:focus-within\) \.gex-desktop-profile-license/);
 assert.match(css,/\.gex-desktop-profile-license \.gex-license-progress-track/);
});
test('native responsive dialog titles use readable standard font weight not 800 bold',()=>{
 assert.match(modalCss,/\.gex-modal-header h2 \{[^}]*font-weight: 500;/);
 assert.doesNotMatch(modalCss,/\.gex-modal-header h2 \{[^}]*font-weight: (?:700|800|900);/);
 assert.match(modalCss,/@media \(max-width: 640px\)[\s\S]*?\.gex-modal-header h2 \{ font-size: 17px;/);
});

test('approved mobile breadcrumb remains in its own fixed bar without duplicating per-page breadcrumbs',()=>{
 assert.match(bar, /<div class="gex-mobile-route-bar"><RouteBreadcrumbs \/><\/div>/);
 assert.match(css, /--gex-mobile-crumb-height: 46px/);
 assert.match(css, /--gex-content-offset: calc\(var\(--gex-appbar-height\) \+ var\(--gex-mobile-crumb-height\)/);
 assert.match(css, /\.gex-mobile-route-bar \{[\s\S]*?position: fixed; display: flex/);
 assert.match(css, /\.goldex-page-container \.mud-breadcrumbs,\s*\.goldex-shell \.goldex-main-content \.mud-breadcrumbs \{ display: none !important;/);
 assert.match(css, /@media \(min-width: 960px\) \{\s*\.gex-mobile-route-bar \{ display: none !important;/);
});