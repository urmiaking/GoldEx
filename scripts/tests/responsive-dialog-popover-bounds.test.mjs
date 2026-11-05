import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const root = '../../src/App/Client/GoldEx.Client.Components/';
const js = readFileSync(new URL(root + 'wwwroot/js/responsive-dialog.js', import.meta.url), 'utf8');
const css = readFileSync(new URL(root + 'Components/Dialogs/ResponsiveDialog.razor.css', import.meta.url), 'utf8');

function fixture({ panel, headerBottom, footerTop, field, listHeight, viewportWidth = 1726, isSelect = true }) {
    const written = new Map(), classes = new Set(['mud-popover-open']);
    const control = { getBoundingClientRect: () => field, closest: selector => selector.startsWith('[inert]') ? null : control };
    const marker = { parentElement: { closest: selector => isSelect && selector.includes('.mud-select') ? control : null } };
    const popup = {
        id: 'popovercontent-test',
        classList: { contains: value => classes.has(value), add: value => classes.add(value) },
        style: { setProperty: (key, value) => written.set(key, value) },
        getBoundingClientRect: () => ({ width: 1132 }),
        querySelector: selector => selector === '.mud-list' ? { scrollHeight: listHeight } : null,
    };
    const modalPanel = {
        contains: node => node === marker,
        getBoundingClientRect: () => panel,
        querySelector: selector => selector === '.gex-modal-header'
            ? { getBoundingClientRect: () => ({ bottom: headerBottom }) }
            : selector.includes('gex-editor-actions') ? { getBoundingClientRect: () => ({ top: footerTop }) } : null
    };
    const dialog = {
        classList: { contains: () => true },
        querySelector: selector => selector === '.gex-modal-panel' ? modalPanel
            : selector === '.gex-modal-overlays' ? { querySelectorAll: () => [popup] } : null
    };
    const ctx = { window: { innerWidth: viewportWidth }, document: { getElementById: id => id === 'popover-test' ? marker : null } };
    vm.runInNewContext(js.replaceAll('export function ', 'function ') + '\nthis.position = placeDialogPopovers;', ctx);
    ctx.position(dialog);
    const num = key => Number.parseFloat(written.get('--gex-popup-' + key));
    return { top: num('top'), left: num('left'), width: num('width'), height: num('height'), classes };
}

test('desktop: dropdown stays immediately under its own 348px field, not the full-width modal', () => {
    const f = fixture({ panel: { left: 0, right: 1132, top: 0, bottom: 635 },
        headerBottom: 93, footerTop: 583,
        field: { left: 18, right: 366, top: 309, bottom: 353, width: 348, height: 44 }, listHeight: 520 });
    assert.equal(f.width, 348);
    assert.equal(f.left, 18);
    assert.equal(f.top, 357);
    assert.ok(f.top + f.height <= 575);
    assert.ok(f.classes.has('gex-dialog-bound-popover'));
});

test('desktop: long dropdown stays below input with internal scroll before footer', () => {
    const f = fixture({ panel: { left: 350, right: 1394, top: 150, bottom: 749 },
        headerBottom: 242, footerTop: 662,
        field: { left: 760, right: 1031, top: 446, bottom: 490, width: 271, height: 44 }, listHeight: 760 });
    assert.equal(f.top, 494);
    assert.equal(f.width, 271);
    assert.ok(f.top + f.height <= 654);
});

test('mobile: long dropdown can flip upward without stretching across dialog', () => {
    const f = fixture({ panel: { left: 6, right: 390, top: 35, bottom: 866 },
        headerBottom: 97, footerTop: 770,
        field: { left: 60, right: 376, top: 556, bottom: 588, width: 316, height: 32 },
        listHeight: 400, viewportWidth: 390 });
    assert.equal(f.width, 316);
    assert.ok(f.top >= 105 && f.top + f.height <= 556, JSON.stringify(f));
});

test('other popovers, such as date pickers, are not modified', () => {
    const f = fixture({ panel: { left: 0, right: 1132, top: 0, bottom: 635 },
        headerBottom: 93, footerTop: 583,
        field: { left: 18, right: 366, top: 309, bottom: 353, width: 348, height: 44 },
        listHeight: 300, isSelect: false });
    assert.ok(Number.isNaN(f.width));
    assert.ok(!f.classes.has('gex-dialog-bound-popover'));
});

test('CSS sets exact popup width and bounds; observers are cleaned up', () => {
    assert.match(js, /const control = container\?\.closest\('\.mud-select, \.mud-autocomplete'\)/);
    assert.match(js, /Math\.min\(anchor\.width, rightMax - leftMin\)/);
    assert.match(js, /state\.stopDialogPopovers\?\.\(\)/);
    assert.match(css, /\.gex-dialog-bound-popover\s*\{[^}]*width: var\(--gex-popup-width\) !important;/);
    assert.match(css, /\.gex-dialog-bound-popover\s*\{[^}]*max-height: var\(--gex-popup-height\) !important;/);
});
