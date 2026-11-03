import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const js = readFileSync(new URL('../../src/App/Client/GoldEx.Client.Components/wwwroot/js/responsive-dialog.js', import.meta.url), 'utf8');
const css = readFileSync(new URL('../../src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor.css', import.meta.url), 'utf8');

function fixture({ panel, headerBottom, footerTop, field, listHeight, width = 280, mobile = false }) {
    const written = new Map(), classes = new Set(['mud-popover-open']);
    const marker = { parentElement: {
        closest: selector => selector.startsWith('[inert]') ? null : {},
        getBoundingClientRect: () => field,
    } };
    const popup = {
        id: 'popovercontent-test',
        classList: { contains: name => classes.has(name), add: name => classes.add(name) },
        style: { setProperty: (name, value) => written.set(name, value) },
        getBoundingClientRect: () => ({ width }),
        querySelector: query => query === '.mud-list' ? { scrollHeight: listHeight } : null,
    };
    const overlay = { querySelectorAll: () => [popup] };
    const modalPanel = {
        contains: node => node === marker,
        getBoundingClientRect: () => panel,
        querySelector: selector => selector === '.gex-modal-header'
            ? { getBoundingClientRect: () => ({ bottom: headerBottom }) }
            : selector.includes('gex-editor-actions') ? { getBoundingClientRect: () => ({ top: footerTop }) } : null,
    };
    const dialog = {
        classList: { contains: () => true },
        querySelector: query => query === '.gex-modal-panel' ? modalPanel : query === '.gex-modal-overlays' ? overlay : null,
    };
    const ctx = { window: { innerWidth: mobile ? 390 : 1726 }, document: { getElementById: id => id === 'popover-test' ? marker : null } };
    vm.runInNewContext(js.replaceAll('export function ', 'function ') + '\nthis.position = placeDialogPopovers;', ctx);
    ctx.position(dialog);
    const num = key => Number.parseFloat(written.get('--gex-popup-' + key));
    return { top: num('top'), left: num('left'), height: num('height'), width: num('width'), classes };
}

test('desktop popup does not overlap action footer', () => {
    const f = fixture({ panel: { left: 350, right: 1394, top: 150, bottom: 749 },
        headerBottom: 242, footerTop: 662, field: { left: 760, right: 1031, top: 446, bottom: 490 },
        listHeight: 420, width: 270 });
    assert.ok(f.top >= 250 && f.top + f.height <= 654, JSON.stringify(f));
    assert.ok(f.left >= 358 && f.left + 270 <= 1386);
    assert.ok(f.classes.has('gex-dialog-bound-popover'));
});

test('mobile popup flips above its field instead of covering bottom buttons', () => {
    const f = fixture({ panel: { left: 6, right: 390, top: 35, bottom: 866 },
        headerBottom: 97, footerTop: 770, field: { left: 60, right: 376, top: 556, bottom: 588 },
        listHeight: 400, width: 310, mobile: true });
    assert.ok(f.top >= 105 && f.top < 556 && f.top + f.height <= 556, JSON.stringify(f));
    assert.ok(f.height <= 340 && f.left >= 14);
});

test('short menu stays below the select', () => {
    const f = fixture({ panel: { left: 350, right: 1394, top: 150, bottom: 748 },
        headerBottom: 242, footerTop: 672, field: { left: 630, right: 910, top: 350, bottom: 396 },
        listHeight: 120, width: 280 });
    assert.equal(f.top, 400);
    assert.ok(f.top + f.height <= 664);
});

test('bounded positioning targets selects only, watches dynamic options, and cleans up', () => {
    assert.match(js, /field\.closest\('\.mud-select, \.mud-autocomplete'\)/);
    assert.match(js, /attributeFilter: \['class'\]/);
    assert.match(js, /state\.stopDialogPopovers\?\.\(\)/);
    assert.match(css, /\.gex-dialog-bound-popover\s*\{[^}]*position: fixed !important;/);
    assert.match(css, /max-height: var\(--gex-popup-height\) !important;/);
});
