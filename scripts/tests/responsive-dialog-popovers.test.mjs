import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';

const root = '../../src/App/Client/GoldEx.Client.Components/';
const read = name => readFileSync(new URL(root + name, import.meta.url), 'utf8');
const markup = read('Components/Dialogs/ResponsiveDialog.razor');
const style = read('Components/Dialogs/ResponsiveDialog.razor.css');
const host = read('Components/Dialogs/ResponsiveDialogHost.razor');

test('shared native dialog renders popover provider outside the clipped panel', () => {
    const panelEnd = markup.indexOf('</section>');
    const overlaysStart = markup.indexOf('<div class="gex-modal-overlays">');
    assert.ok(panelEnd >= 0 && overlaysStart > panelEnd, 'overlay must be sibling of modal panel');
    assert.match(markup.slice(overlaysStart), /@Overlays\s*<\/div>\s*<\/dialog>/);
    assert.match(host, /<MudPopoverProvider\s*\/>/);
    assert.match(host, /<MudSnackbarProvider\s*\/>/);
});

test('popover overlay is stacked above the panel and preserves MudBlazor positioning', () => {
    const panel = style.match(/\.gex-modal-panel\s*\{([^}]*)\}/)?.[1];
    const overlay = style.match(/\.gex-modal-layer\.is-active \.gex-modal-overlays\s*\{([^}]*)\}/)?.[1];
    assert.ok(panel && overlay);
    assert.match(panel, /z-index:\s*1/);
    assert.match(panel, /overflow:\s*hidden/);
    assert.match(overlay, /z-index:\s*5/);
    assert.match(overlay, /pointer-events:\s*none/);
    assert.match(style, /\.gex-modal-overlays ::deep \.mud-popover-open/);
    assert.match(style, /\.gex-modal-overlays ::deep \.mud-popover-open,[\s\S]*?pointer-events:\s*auto/);
    assert.doesNotMatch(style, /\.mud-popover\s*\{\s*position:\s*fixed/);
    assert.match(style, /\.gex-modal-layer:not\(\.is-active\) \.gex-modal-overlays\s*\{\s*display:\s*contents/);
});

test('select lists remain scrollable and viewport bounded on desktop and mobile', () => {
    assert.match(style, /\.gex-modal-overlays ::deep \.mud-popover\s*\{\s*max-height:/);
    assert.match(style, /\.gex-modal-overlays ::deep \.mud-popover \.mud-list\s*\{[^}]*overflow-y:\s*auto/);
    assert.match(style, /@media \(max-width: 640px\)/);
    assert.match(style, /\.gex-modal-backdrop\s*\{[^}]*backdrop-filter:\s*none/);
    assert.match(markup, /<dialog\s+class="gex-modal-layer/);
    assert.match(markup, /<section\s+class="gex-modal-panel/);
});
