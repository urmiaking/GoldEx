import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const file = p => readFileSync(new URL('../../' + p, import.meta.url), 'utf8');
const css = file('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
const palette = file('src/App/Client/GoldEx.Client.Components/Themes/ColorPalettes.cs');
const component = file('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const start = palette.indexOf('private static MudTheme CreateGoldTheme()');
const gold = palette.slice(start, palette.indexOf('private static MudTheme Create(', start));
const light = gold.split('theme.PaletteLight = new PaletteLight')[1].split('theme.PaletteDark = new PaletteDark')[0];
const dark = gold.split('theme.PaletteDark = new PaletteDark')[1].split('return theme;')[0];

const value = (text, key) => {
    const match = text.match(new RegExp('\\b' + key + '\\s*=\\s*"(#[0-9a-fA-F]{6})"'));
    assert.ok(match, 'Missing ' + key);
    return match[1];
};
const luminance = hex => {
    const rgb = [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255)
        .map(n => n <= .04045 ? n / 12.92 : ((n + .055) / 1.055) ** 2.4);
    return .2126 * rgb[0] + .7152 * rgb[1] + .0722 * rgb[2];
};
const contrast = (a, b) => {
    const x = luminance(a);
    const y = luminance(b);
    return (Math.max(x, y) + .05) / (Math.min(x, y) + .05);
};

test('mobile amount labels never apply muted color to paid status chips', () => {
    // Regression: broad selector matched all spans, making status gray-on-green.
    assert.doesNotMatch(css, /\.invoice-card-amounts\s*>\s*div\s+span\s*\{/);
    assert.match(css, /\.invoice-card-amounts\s*>\s*div\s*>\s*span:not\(\.invoice-paid\)\s*\{[^}]*color:\s*var\(--mud-palette-text-secondary\)/);
    assert.match(css, /\.invoice-card-amounts\s+\.invoice-paid,\s*\.invoice-balance\s+\.invoice-paid\s*\{\s*color:\s*var\(--mud-palette-success-text,\s*#FFFFFF\)/);
    assert.match(css, /\.invoice-paid\s+\.gex-shell-icon\s*\{[^}]*color:\s*inherit;\s*stroke:\s*currentColor;/);
    assert.equal(component.split('class="invoice-paid"').length - 1, 2);
});

test('Gold light and dark success-chip foreground tokens meet WCAG AA', () => {
    const lightFill = value(light, 'Success');
    const lightText = value(light, 'SuccessContrastText');
    const darkFill = value(dark, 'Success');
    const darkText = value(dark, 'SuccessContrastText');
    assert.equal(lightText.toUpperCase(), '#FFFFFF');
    assert.equal(darkText.toUpperCase(), '#151D23');
    assert.ok(contrast(lightFill, lightText) >= 4.5, 'Light green chip text contrast ' + contrast(lightFill, lightText));
    assert.ok(contrast(darkFill, darkText) >= 4.5, 'Dark green chip text contrast ' + contrast(darkFill, darkText));
});
