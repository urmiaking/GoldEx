import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read = path => readFileSync(new URL('../../'+path,import.meta.url),'utf8');
const comp='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const route=read(comp+'RouteBreadcrumbs.razor'), nav=read(comp+'NavMenu.razor');
const notes=read(comp+'ReleaseNotesMenuItem.razor');
const home=read('src/App/Client/GoldEx.Client/Pages/Home/Index.razor');
const homeCode=read('src/App/Client/GoldEx.Client/Pages/Home/Index.razor.cs');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const dock=read('src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js');
const navigation=read('src/App/Client/GoldEx.Client.Components/wwwroot/js/navigation.js');
test('RTL trail uses left arrow with isolated direction',()=>{
 assert.match(route,/gex-route-trail-separator[^>]*>←<\/span>/);
 assert.doesNotMatch(route,/gex-route-trail-separator[^>]*>[‹›]<\/span>/);
 assert.match(css,/\.gex-route-trail-separator \{ direction: ltr; unicode-bidi: isolate; \}/);
});
test('program updates moved from Home to NavMenu without losing automatic popup',()=>{
 assert.doesNotMatch(home,/OpenReleaseNotesManuallyAsync|release-notes-btn|تغییرات برنامه/);
 assert.doesNotMatch(homeCode,/private async Task OpenReleaseNotesManuallyAsync/);
 assert.match(homeCode,/ShowReleaseDialogAsync\(current, lastSeen\)/);
 assert.match(nav,/<ReleaseNotesMenuItem\s*\/>/);
 assert.match(notes,/DialogParameters<ReleaseHistory>/);
 assert.match(notes,/ShowAsync<ReleaseHistory>/);
});
test('dock includes the release button in overflow and hover calculations',()=>{
 assert.match(dock,/:scope > a\[href\], :scope > button\.gex-nav-release/);
 assert.match(dock,/\.gex-navigation-menu > button\.gex-nav-release/);
 assert.match(css,/\.gex-liquid-dock \.gex-navigation-menu > button\.gex-nav-release:is\(:hover,:focus-visible\)/);
 assert.match(css,/\.gex-liquid-dock \.gex-dock-overflow-links > button\.gex-nav-release/);
 assert.match(dock,/original\.click\(\)/);
});
test('account logout remains within curved glass and dark mode keeps integrated tint',()=>{
 assert.match(dock,/width - 48 \+ \(isAccount \? 44 : 0\)/);
 assert.match(css,/inset-inline-start: calc\(100% - 12px\)/);
 assert.match(css,/width: min\(var\(--gex-item-expanded-width, 159px\), 174px\)/);
 assert.match(css,/html\[data-gex-mode="dark"\] \.gex-liquid-dock\.gex-dock-is-account \.gex-dock-glass/);
});
test('desktop body top gap matches dock 26px, mobile preserves original 12px',()=>{
 assert.match(css,/--gex-content-gap: 12px;/);
 assert.match(css,/inset-block-start: calc\(var\(--gex-appbar-height\) \+ 26px/);
 assert.match(css,/@media \(min-width: 960px\) \{\s*\.goldex-shell \{ --gex-content-gap: 26px; \}/);
});
test('store-picker click-away, Escape and disposal are wired',()=>{
 assert.match(navigation,/document\.addEventListener\('pointerdown', dismissStore, true\)/);
 assert.match(navigation,/event\.target\.closest\?\.\('\.gex-store-picker'\)/);
 assert.match(navigation,/trigger\.click\(\)/);
 assert.match(navigation,/document\.removeEventListener\('pointerdown', dismissStore, true\)/);
});
