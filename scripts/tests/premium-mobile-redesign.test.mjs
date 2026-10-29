import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const root='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const bar=read(root+'AppBar.razor'), menu=read(root+'NavMenu.razor');
const drawer=read(root+'Drawer.razor'), profile=read(root+'ShellIdentity.razor');
const license=read(root+'MobileLicenseRibbon.razor');
const invoice=read('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor');
const customers=read('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerFiltersDialog.razor');
const report=read('src/App/Client/GoldEx.Client/Pages/Reporting/Components/ReportFiltersBase.razor');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

test('mobile brand is independent of controls; settings, theme, notifications, Help remain available',()=>{
 assert.match(bar,/<div class="gex-mobile-brand"><ShellBrand \/><\/div>/);
 assert.match(bar,/gex-menu-toggle/);
 assert.match(bar,/<AppearanceControls ModeOnly="true" \/>/);
 assert.match(bar,/<AuthorizeView Roles="Administrators,Owners">[\s\S]*?OnNotificationsRequested/);
 assert.match(bar,/<AuthorizeView><Authorized><Settings \/><\/Authorized><\/AuthorizeView>/);
 assert.match(menu,/<NavLink href="@ClientRoutes\.Blogs\.Index">.*راهنما/);
 assert.match(css,/\.gex-mobile-brand \{[\s\S]*?left: 50%;[\s\S]*?transform: translate\(-50%, -50%\)/);
 assert.match(css,/@media \(max-width: 359\.98px\)/);
});

test('mobile trail appears under header at body gutter with page-level duplicates hidden',()=>{
 assert.match(bar,/<\/header>\s*<div class="gex-mobile-route-bar"><RouteBreadcrumbs \/><\/div>/);
 assert.match(css,/--gex-mobile-crumb-height: 46px/);
 assert.match(css,/--gex-content-offset: calc\(var\(--gex-appbar-height\) \+ var\(--gex-mobile-crumb-height\)/);
 assert.match(css,/\.gex-mobile-route-bar \{[\s\S]*?top: calc\(var\(--gex-appbar-height\)/);
 assert.match(css,/\.goldex-page-container \.mud-breadcrumbs,\s*\.goldex-shell \.goldex-main-content \.mud-breadcrumbs \{ display: none !important;/);
 assert.match(css,/@media \(min-width: 960px\) \{\s*\.gex-mobile-route-bar \{ display: none !important;/);
});

test('secondary routes grouped under More and release notes never duplicated',()=>{
 assert.match(menu,/gex-mobile-more-heading[\s\S]*?ClientRoutes.Settings.Index[\s\S]*?ClientRoutes.Blogs.Index/);
 assert.equal((menu.match(/<ReleaseNotesMenuItem \/>/g)||[]).length,1);
 assert.ok(menu.indexOf('دربارهٔ گلدکس') < menu.indexOf('<ReleaseNotesMenuItem />'));
 assert.match(css,/\.gex-mobile-more-heading \{[\s\S]*?display: flex/);
 assert.match(drawer,/<ShellIdentity \/>/);
});

test('profile license ribbon restores current license status with expiry and progress',()=>{
 assert.match(profile,/<div class="gex-mobile-license"><MobileLicenseRibbon \/><\/div>/);
 assert.match(license,/LicenseState\.Current/);
 assert.match(license,/LicenseState\.OnChange \+= OnLicenseChanged/);
 assert.match(license,/LicenseState\.OnChange -= OnLicenseChanged/);
 assert.match(license,/LicensePlan\.Regular/);
 assert.match(license,/LicensePlan\.Trial/);
 assert.match(license,/LicensePlan\.Unregistered/);
 assert.match(license,/ExpireDate\.Date/);
 assert.match(license,/ClientRoutes.RegisterProduct.Extend/);
 assert.match(license,/ClientRoutes.RegisterProduct.Upgrade/);
 assert.match(css,/\.gex-sidebar-identity \{[\s\S]*?flex-direction: column/);
 assert.match(css,/\.gex-license-ribbon \{/);
});

test('date filters inherit report picker API and premium field style without breaking callbacks',()=>{
 for(const [name,source] of [['invoice',invoice],['customer',customers]]) {
  for(const property of ['Rounded="true"','MaxMonthColumns="1"','DisplayMonths="1"',
   'ShowToolbar="false"','AnchorOrigin="Origin.BottomCenter"','TransformOrigin="Origin.TopCenter"',
   'RelativeWidth="DropdownWidth.Adaptive"','PlaceholderStart="از تاریخ"','PlaceholderEnd="تا تاریخ"']) {
   assert.ok(source.includes(property),name+': missing '+property);
  }
  assert.match(source,/DateRangeChanged="@\(dr => _dateRange = dr\)"/);
  assert.match(source,/gex-filter-date-card/);
  assert.match(source,/class="gex-directory-filter-actions"/);
  assert.match(source,/Class="gex-filter-mobile-sort"/);
 }
 assert.match(report,/RelativeWidth="DropdownWidth.Adaptive"/);
 assert.match(css,/\.gex-filter-date-card \{/);
 assert.match(css,/\.gex-filter-date-card \.mud-input-outlined \{/);
 assert.match(css,/\.gex-directory-filter-actions \{/);
});
