import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const load = relative => readFileSync(new URL('../../' + relative, import.meta.url), 'utf8');
const palette = load('src/App/Client/GoldEx.Client.Components/Themes/ColorPalettes.cs');
const gold = palette.split('private static MudTheme CreateGoldTheme()')[1].split('private static MudTheme Create(')[0];
const light = gold.split('theme.PaletteLight = new PaletteLight')[1].split('theme.PaletteDark = new PaletteDark')[0];
const invoices = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor');
const customer = load('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerCard.razor');
const paths = [
    'src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomerCard.razor.css',
    'src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor.css',
    'src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css',
    'src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor.css'
];

function color(name) {
    const found = light.match(new RegExp('\\b' + name + '\\s*=\\s*"(#[0-9a-fA-F]{6})"'));
    assert.ok(found, 'Gold light palette missing ' + name);
    return found[1];
}
function luminance(hex) {
    const c = [1, 3, 5].map(i => Number.parseInt(hex.slice(i,i+2), 16) / 255)
        .map(n => n <= .04045 ? n / 12.92 : ((n + .055) / 1.055) ** 2.4);
    return c[0] * .2126 + c[1] * .7152 + c[2] * .0722;
}
function contrast(a,b) {
    const [hi,lo] = [luminance(a),luminance(b)].sort((x,y)=>y-x);
    return (hi + .05) / (lo + .05);
}
test('Gold semantic colors pass normal-text WCAG contrast on white and brand background', () => {
    for (const key of ['Primary','Secondary','TextSecondary','Success','Info','Warning','Error']) {
        const value = color(key);
        assert.ok(contrast(value,'#FFFFFF') >= 4.5, key + ' '+ value + ' lacks contrast on white');
        assert.ok(contrast(value,'#F5F3EE') >= 4.5, key + ' ' + value + ' lacks contrast on warm background');
    }
    assert.ok(contrast(color('Primary'), color('PrimaryContrastText')) >= 4.5);
});
test('directory headings and invoice table use standard Mud typography classes', () => {
    const invoiceCss = load('src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoicesList.razor.css');
    const customers = load('src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor');
    assert.match(invoices, /invoice-table mud-typography-body2/);
    assert.match(invoices, /mud-typography-h5/);
    assert.match(customers, /mud-typography-h5/);
    assert.match(customer, /mud-typography-subtitle1/);
    assert.match(invoiceCss, /\.invoice-customer-balance ::deep \.mud-typography \{[^}]*font-size: \.875rem; font-weight: 500/);
});
test('Both directories avoid synthetic-heavy or nonstandard 700-900 font weights', () => {
    for (const path of paths) {
        const css = load(path);
        assert.doesNotMatch(css, /font-weight:\s*(?:7\d\d|8\d\d|9\d\d)\b/, path);
    }
});
test('customer balance is a live value in invoice table, not just a details button', () => {
    const tableStart = invoices.indexOf('<table class="invoice-table');
    const tableEnd = invoices.indexOf('</table>',tableStart);
    const table = invoices.slice(tableStart,tableEnd);
    assert.match(table, /<td class="invoice-customer-balance">[\s\S]*?<CustomerRemaining[\s\S]*?CustomerId="invoice\.CustomerId"/);
    assert.match(table, /EnableManualSlide="true"/);
    assert.doesNotMatch(table, /مشاهده مانده|invoice-text-action/);
    assert.match(invoices, /<InvoiceDetails/);
});


test('Gold dark palette uses dark lettering on light-colored semantic buttons', () => {
    const dark = gold.split('theme.PaletteDark = new PaletteDark')[1].split('return theme')[0];
    for (const key of ['Primary','Secondary','Info','Success','Warning','Error']) {
        const colorMatch = dark.match(new RegExp('\\b' + key + '\\s*=\\s*"(#[0-9a-fA-F]{6})"'));
        const textMatch = dark.match(new RegExp('\\b' + key + 'ContrastText\\s*=\\s*"(#[0-9a-fA-F]{6})"'));
        assert.ok(colorMatch && textMatch, key + ' is missing contrast configuration');
        assert.ok(contrast(colorMatch[1], textMatch[1]) >= 4.5, key + ' has low contrast in dark mode');
    }
});