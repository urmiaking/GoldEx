import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const root = '../../src/App/Client/GoldEx.Client.Components/';
const js = readFileSync(new URL(root + 'wwwroot/js/responsive-dialog.js', import.meta.url), 'utf8');
const css = readFileSync(new URL(root + 'Components/Dialogs/ResponsiveDialog.razor.css', import.meta.url), 'utf8');

function fixture({ panel, headerBottom, footerTop, field, listHeight, viewportWidth = 1726, viewportHeight = 1000, visualViewport, isSelect = true, focusedAutocomplete = false, missingMarker = false }) {
    const written = new Map(), priorities = new Map(), classes = new Set(['mud-popover-open']);
    const control = { getBoundingClientRect: () => field, closest: selector => selector.startsWith('[inert]') ? null : control };
    const marker = { parentElement: { closest: selector => isSelect && selector.includes('.mud-select') ? control : null } };
    const popup = {
        id: 'popovercontent-test',
        classList: { contains: value => classes.has(value), add: value => classes.add(value) },
        style: { setProperty: (key, value, priority) => { written.set(key, value); priorities.set(key, priority); } },
        getBoundingClientRect: () => ({ width: 1132 }),
        querySelector: selector => selector.startsWith('.mud-list') ? { scrollHeight: listHeight } : null,
    };
    const modalPanel = {
        contains: node => node === control || (!missingMarker && node === marker),
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
    const ctx = { window: { innerWidth: viewportWidth, innerHeight: viewportHeight, visualViewport }, document: { getElementById: id => !missingMarker && id === 'popover-test' ? marker : null, activeElement: focusedAutocomplete ? { closest: () => control } : null } };
    vm.runInNewContext(js.replaceAll('export function ', 'function ') + '\nthis.position = placeDialogPopovers;', ctx);
    ctx.position(dialog);
    const num = key => Number.parseFloat(written.get('--gex-popup-' + key));
    return { top: num('top'), left: num('left'), width: num('width'), height: num('height'), classes, styles: written, priorities };
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
    assert.equal(f.styles.get('position'), 'fixed');
    assert.equal(f.styles.get('top'), f.top + 'px');
    assert.equal(f.styles.get('left'), f.left + 'px');
    assert.equal(f.styles.get('width'), f.width + 'px');
    assert.equal(f.styles.get('transform'), 'none');
    assert.equal(f.priorities.get('top'), 'important');
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
    assert.match(js, /const popovers = \[\.\.\.overlay\.querySelectorAll\('\.mud-popover-open'\)\]/);
    assert.match(js, /const focused = document\.activeElement\?\.closest\?\./);
    assert.match(js, /popup\.style\.setProperty\(name, value, 'important'\)/);
    assert.match(js, /Math\.min\(anchor\.width, rightMax - leftMin\)/);
    assert.match(js, /state\.stopDialogPopovers\?\.\(\)/);
    assert.match(css, /\.gex-dialog-bound-popover\s*\{[^}]*width: var\(--gex-popup-width\) !important;/);
    assert.match(css, /\.gex-dialog-bound-popover\s*\{[^}]*max-height: var\(--gex-popup-height\) !important;/);
});

test('mobile autocomplete: missing portal marker uses its focused input inside the modal', () => {
    const f = fixture({ panel: { left: 8, right: 382, top: 18, bottom: 850 },
        headerBottom: 84, footerTop: 810,
        field: { left: 30, right: 350, top: 468, bottom: 510, width: 320, height: 42 },
        listHeight: 370, viewportWidth: 390, isSelect: false, focusedAutocomplete: true, missingMarker: true });
    assert.equal(f.width, 320);
    assert.ok(f.top >= 92 && f.top + f.height <= 802, JSON.stringify(f));
    assert.ok(f.classes.has('gex-dialog-bound-popover'));
});

test('mobile keyboard: autocomplete results remain inside the visual viewport and flip upward', () => {
    const f = fixture({ panel: { left: 8, right: 382, top: 15, bottom: 850 },
        headerBottom: 82, footerTop: 795,
        field: { left: 26, right: 348, top: 485, bottom: 529, width: 322, height: 44 },
        listHeight: 420, viewportWidth: 390, viewportHeight: 850,
        visualViewport: { offsetTop: 0, offsetLeft: 0, width: 390, height: 620 },
        isSelect: false, focusedAutocomplete: true, missingMarker: true });
    assert.equal(f.width, 322);
    assert.ok(f.top >= 90, JSON.stringify(f));
    assert.ok(f.top + f.height <= 612, JSON.stringify(f));
    assert.ok(f.top + f.height <= 485, 'suggestions should flip above the text field');
});

test('unrelated popovers do not attach to inactive/autocomplete controls', () => {
    const f = fixture({ panel: { left: 8, right: 382, top: 15, bottom: 850 },
        headerBottom: 82, footerTop: 795,
        field: { left: 26, right: 348, top: 485, bottom: 529, width: 322, height: 44 },
        listHeight: 420, viewportWidth: 390, isSelect: false, missingMarker: true });
    assert.ok(Number.isNaN(f.width));
    assert.ok(!f.classes.has('gex-dialog-bound-popover'));
});