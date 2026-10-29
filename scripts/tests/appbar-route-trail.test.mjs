import assert from 'node:assert/strict';
import {readFileSync, readdirSync} from 'node:fs';
import {test} from 'node:test';

const read = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const prefix = 'src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const bar = read(prefix + 'AppBar.razor');
const clock = read(prefix + 'Clock.razor');
const trail = read(prefix + 'RouteBreadcrumbs.razor');
const css = read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

function walk(path) {
    return readdirSync(new URL('../../' + path, import.meta.url), {withFileTypes:true})
        .flatMap(entry => entry.isDirectory() ? walk(path + '/' + entry.name) :
            entry.name.endsWith('.razor') ? [path + '/' + entry.name] : []);
}

test('desktop AppBar retains the route trail inside the header and date at left', () => {
    const crumb = bar.indexOf('<RouteBreadcrumbs />');
    const actions = bar.indexOf('class="gex-topbar-actions"');
    const date = bar.indexOf('class="gex-topbar-date"');
    assert.ok(crumb > bar.indexOf('gex-desktop-mark') && crumb < actions);
    assert.ok(actions < date && date < bar.indexOf('</header>'));
    assert.doesNotMatch(bar, /MudBreadcrumbs/);
    assert.match(css, /\.gex-route-trail\s*\{\s*display: none;/);
    assert.match(css, /@media \(min-width: 960px\)[\s\S]*?\.gex-route-trail\s*\{\s*display: flex;/);
    assert.match(css, /\.goldex-page-container \.mud-breadcrumbs\s*\{\s*display: none !important;/);
    assert.match(css, /\.goldex-page-container \.d-none\.d-md-block:has\(> \.mud-breadcrumbs\)/);
});

test('custom route trail covers nested routes, updates on navigation and hides URL identifiers', () => {
    assert.match(trail, /Navigation\.LocationChanged \+= OnLocationChanged/);
    assert.match(trail, /Navigation\.LocationChanged -= OnLocationChanged/);
    assert.match(trail, /Navigation\.ToBaseRelativePath\(Navigation\.Uri\)\.Split\('\?', '#'\)/);
    assert.match(trail, /new Crumb\("صفحه اصلی"/);
    assert.match(trail, /Labels\.TryGetValue\(path, out var label\)/);
    assert.match(trail, /Dynamic identifiers/);
    assert.match(trail, /if \(_items\.Count > 1\)\s*break;/);
    assert.match(trail, /_items\[\^1\].*Href = null/);
    assert.match(trail, /@key="_location"/);
    for (const route of ['dashboard', 'calculator', 'invoices/list', 'invoices/set',
        'invoices/viewer', 'customers', 'reporting/profit-analysis', 'base-info/price-settings',
        'inventory-stocks/inventory-entry/list', 'finances/checks', 'user-accounts/passkeys']) {
        assert.ok(trail.includes('["/' + route + '"]'), 'Missing route: ' + route);
    }
});

test('all existing desktop page breadcrumbs are hidden without removing mobile breadcrumb markup', () => {
    const pages = walk('src/App/Client/GoldEx.Client/Pages');
    const count = pages.reduce((total,file) => total + (read(file).match(/<MudBreadcrumbs\b/g) || []).length, 0);
    assert.ok(count >= 45, 'Expected page-local mobile breadcrumbs, found ' + count);
    assert.match(css, /@media \(min-width: 960px\)[\s\S]*?\.goldex-page-container \.mud-breadcrumbs\s*\{\s*display: none !important;/);
});

test('missing Help has no phantom gap, and Help precedes dark-mode button in RTL desktop', () => {
    assert.ok(bar.indexOf('<AppearanceControls ModeOnly="true" />') < bar.indexOf('<div class="gex-topbar-help"><Help /></div>'));
    assert.match(css, /\.gex-topbar-help\s*\{\s*display: contents;/);
    assert.match(css, /\.gex-topbar-help > \.gex-icon-button\s*\{\s*order: -1;/);
    assert.match(css, /@media \(max-width: 599\.98px\)[\s\S]*?\.gex-topbar-help\s*\{\s*display: none;/);
});

test('desktop date uses full Jalali weekday and date, with clock on a second line', () => {
    assert.match(clock, /PersianCalendar Calendar = new\(\)/);
    assert.match(clock, /Calendar\.GetDayOfMonth\(now\)/);
    assert.match(clock, /Calendar\.GetMonth\(now\)/);
    assert.match(clock, /Calendar\.GetYear\(now\)/);
    assert.match(clock, /WeekdayNames\[\(int\)now\.DayOfWeek\]/);
    assert.match(clock, /"اردیبهشت"/);
    assert.match(clock, /gex-clock-date/);
    assert.match(clock, /gex-clock-time/);
    assert.doesNotMatch(clock, /ShellIcon/);
    assert.match(css, /\.gex-topbar \.gex-clock\s*\{\s*display: flex; flex-direction: column;/);
    assert.match(css, /\.gex-topbar \.gex-clock-time\s*\{[\s\S]*?display: flex;/);
});
