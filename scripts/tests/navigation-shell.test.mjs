import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

function source(relativePath) {
    return readFileSync(new URL(`../../${relativePath}`, import.meta.url), 'utf8');
}

const mainNav = source('src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor');
const drawer = source('src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor');
const desktopSettings = source('src/App/Client/GoldEx.Client/Pages/Settings/Shared/SettingsNavMenu.razor');
const mobileSettings = source('src/App/Client/GoldEx.Client.Components/Layouts/Components/Settings.razor');
const styles = source('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

function settingsRoutes(markup) {
    return [...markup.matchAll(/ClientRoutes\.[A-Za-z0-9_.]+/g)].map(match => match[0]).sort();
}

test('main and desktop settings navigation use native links instead of MudBlazor navigation', () => {
    for (const markup of [mainNav, desktopSettings]) {
        assert.match(markup, /<nav\b/);
        assert.match(markup, /<NavLink\b/);
        assert.doesNotMatch(markup, /<MudNav(?:Menu|Link)\b/);
    }
    assert.match(mainNav, /<ShellIcon\b/);
    assert.match(desktopSettings, /<ShellIcon\b/);
});

test('desktop and mobile settings offer the same destinations and keep role guards', () => {
    assert.deepEqual(settingsRoutes(desktopSettings), settingsRoutes(mobileSettings));
    assert.equal(settingsRoutes(desktopSettings).length, 17);
    for (const markup of [desktopSettings, mobileSettings])
        assert.match(markup, /AuthorizeView Roles="@BuiltinRoles\.Administrators"/);
});

test('navigation appears before the store request and drawer synchronizes only on state changes', () => {
    assert.match(mainNav, /OnAfterRenderAsync\(bool firstRender\)/);
    assert.match(mainNav, /if \(!firstRender \|\| User\?\.Identity\?\.IsAuthenticated != true\) return;/);
    assert.doesNotMatch(mainNav, /await LoadStoresAsync\(\);[\s\S]*?<nav/);
    assert.match(drawer, /_lastSyncedOpen != IsDrawerOpen/);
    assert.match(drawer, /if \(!IsDrawerOpen\) return;/);
});

test('mobile logo receives its own two-pixel logical gap and navigation is responsive', () => {
    assert.match(styles, /@media \(max-width: 959\.98px\)[\s\S]*?\.gex-mobile-brand\s*\{[^}]*margin-inline-start:\s*2px/);
    assert.match(styles, /\.gex-settings-navigation-section a\.active/);
    assert.match(styles, /@media \(prefers-reduced-motion: reduce\)/);
});
