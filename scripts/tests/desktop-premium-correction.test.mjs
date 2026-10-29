import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const root='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const bar=read(root+'AppBar.razor'),identity=read(root+'ShellIdentity.razor'),nav=read(root+'NavMenu.razor');
const license=read(root+'DesktopLicenseRibbon.razor'),indicator=read(root+'LicenseIndicator.razor');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const modal=read('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor.css');
const invoices=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor');
const customers=read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerFiltersDialog.razor');

test('desktop: icon on right, GOLDEx text centered, breadcrumb in original AppBar before actions/date',()=>{
 assert.match(bar,/class="gex-desktop-mark"[\s\S]*?apple-touch-icon-180x180.png/);
 assert.match(bar,/<a class="gex-desktop-wordmark"[^>]*>GoldEx<\/a>/);
 assert.match(bar,/<div class="gex-desktop-route-trail"><RouteBreadcrumbs \/><\/div>/);
 assert.ok(bar.indexOf('gex-desktop-mark')<bar.indexOf('gex-desktop-route-trail'));
 assert.ok(bar.indexOf('gex-desktop-route-trail')<bar.indexOf('gex-topbar-actions'));
 assert.ok(bar.indexOf('gex-topbar-actions')<bar.indexOf('gex-topbar-date'));
 assert.doesNotMatch(bar,/gex-desktop-route-bar/);
 assert.match(css,/\.goldex-shell \.gex-desktop-wordmark \{[\s\S]*?left:50%;[\s\S]*?transform:translate\(-50%,-50%\)/);
 assert.match(css,/\.goldex-shell \.gex-desktop-route-trail \{[\s\S]*?width:min\(480px,calc\(50% - 145px\)\)/);
 assert.match(css,/\.goldex-shell \.gex-topbar-actions \{[\s\S]*?margin-inline-start:auto; margin-inline-end:0/);
 assert.match(bar,/<AppearanceControls ModeOnly="true" \/>/);
 assert.match(bar,/<div class="gex-topbar-help"><Help \/><\/div>/);
 assert.match(bar,/<Clock \/>/);
});
test('mobile breadcrumb unchanged; filters and logo retain previous mobile UI',()=>{
 assert.match(bar,/<div class="gex-mobile-brand"><ShellBrand Compact="true" \/><\/div>/);
 assert.match(bar,/<div class="gex-mobile-route-bar"><RouteBreadcrumbs \/><\/div>/);
 assert.match(css,/--gex-mobile-crumb-height: 46px/);
 assert.match(css,/\.goldex-page-container \.mud-breadcrumbs,\s*\.goldex-shell \.goldex-main-content \.mud-breadcrumbs \{ display: none !important;/);
 for(const source of [invoices,customers]){
   assert.match(source,/Class="w-100 gex-desktop-filter-date"/);
   assert.match(source,/Adornment="Adornment.Start"/);
   assert.doesNotMatch(source,/gex-filter-date-card|PlaceholderStart="از تاریخ"/);
 }
});
test('license: AppBar icon hover/focus below and mobile single compact bottom line',()=>{
 assert.match(bar,/<Authorized><LicenseIndicator \/><\/Authorized>/);
 assert.match(indicator,/class="gex-license-indicator"/);
 assert.match(indicator,/class="gex-license-details"/);
 assert.match(indicator,/<DesktopLicenseRibbon \/>/);
 assert.doesNotMatch(identity,/gex-desktop-profile-license/);
 assert.match(identity,/<div class="gex-mobile-license"><DesktopLicenseRibbon \/><\/div>/);
 assert.match(license,/LicenseState\.OnChange \+= OnLicenseChanged/);
 assert.match(license,/LicenseState\.OnChange -= OnLicenseChanged/);
 assert.match(license,/ClientRoutes.RegisterProduct.Extend/);
 assert.match(css,/\.gex-license-indicator:is\(:hover,:focus-within\) \.gex-license-details/);
 assert.match(css,/\.gex-license-indicator:has\(\.gex-license-ribbon.is-expired\) \.gex-license-indicator-dot/);
 assert.match(css,/\.gex-mobile-license :is\(\.gex-license-ribbon-caption,\.gex-license-progress-track\) \{ display:none; \}/);
});
test('mobile release notes aligns to the right and dialog title weight is 500',()=>{
 assert.match(nav,/<ReleaseNotesMenuItem \/>/);
 assert.match(css,/\.gex-sidebar \.gex-navigation-menu > button.gex-nav-release \{[\s\S]*?justify-content:flex-start;[\s\S]*?direction:rtl; text-align:right/);
 assert.match(css,/\.gex-sidebar \.gex-navigation-menu > button.gex-nav-release > span \{ flex:1 1 auto; text-align:right/);
 assert.match(modal,/\.gex-modal-header h2 \{[^}]*font-weight: 500;/);
});
