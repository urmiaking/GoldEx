import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const readHome = (file) => readFileSync(new URL(`../../src/App/Client/GoldEx.Client/Pages/Home/${file}`, import.meta.url), 'utf8');

test('authenticated mobile home keeps live price board before follow-up cards, without desktop tabs', () => {
    const markup = readHome('Index.razor');
    const mobile = markup.split('<MudHidden Breakpoint="Breakpoint.MdAndUp">')[1]?.split('</MudHidden>')[0];
    assert.ok(mobile, 'dedicated mobile branch must exist');
    assert.match(mobile, /<PriceBoard\b/);
    assert.match(mobile, /<HomeOperations\s*\/>/);
    assert.ok(mobile.indexOf('<PriceBoard') < mobile.indexOf('<HomeOperations'), 'live prices should come first');
    assert.doesNotMatch(mobile, /gex-home-launcher|gex-home-tabs|<HomeStatistics\b/);
});

test('desktop continues to display its existing dashboard and operational panel', () => {
    const markup = readHome('Index.razor');
    const desktop = markup.split('<MudHidden Breakpoint="Breakpoint.SmAndDown">')[1]?.split('</MudHidden>')[0];
    assert.ok(desktop);
    assert.match(desktop, /gex-home-launcher/);
    assert.match(desktop, /gex-home-tabs/);
    assert.match(desktop, /<HomeOperations\s*\/>/);
    assert.match(desktop, /<HomeStatistics\s*\/>/);
});

test('mobile work queue retains both cards and stacks on phones and tablets', () => {
    const markup = readHome('Components/HomeOperations.razor');
    const styles = readHome('Components/HomeOperations.razor.css');
    const homeStyles = readHome('Index.razor.css');
    assert.match(markup, /فاکتورهای تسویه‌نشده/);
    assert.match(markup, /پیگیری امور فروشگاه/);
    assert.match(styles, /@media\s*\(max-width:959px\).*?grid-template-columns:1fr/s);
    assert.match(homeStyles, /\.gex-home-mobile-operations\s*\{\s*margin-top:24px/);
});
