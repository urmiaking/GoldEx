import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { bind } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/quick-create.js';

function target() {
    const listeners = new Map();
    return { listeners, addEventListener: (type, fn) => listeners.set(type, fn), removeEventListener: type => listeners.delete(type),
        emit(type, values = {}) { listeners.get(type)?.(values); } };
}
function environment(mobile = true) {
    const classes = new Set(), attributes = new Map();
    let paletteClosed = false, opens = 0;
    const media = Object.assign(target(), { matches: mobile });
    globalThis.matchMedia = () => media;
    globalThis.document = { documentElement: { classList: { add: value => classes.add(value), remove: value => classes.delete(value) } } };
    const trigger = Object.assign(target(), { setAttribute: (key, value) => attributes.set(key, value),
        closest: () => ({ querySelectorAll: () => [{ hidePopover() { paletteClosed = true; } }] }) });
    const panel = Object.assign(target(), { open: false, closest: () => null,
        showModal() { this.open = true; opens++; }, close() { this.open = false; },
        getBoundingClientRect: () => ({ left: 12, right: 348, top: 200, bottom: 620 }) });
    return { binding: bind(trigger, panel), trigger, panel, media, classes, attributes,
        opens: () => opens, paletteClosed: () => paletteClosed };
}

test('mobile plus opens one native modal, closes palette overlays and exposes expanded state', () => {
    const e = environment();
    e.trigger.emit('click'); e.trigger.emit('click');
    assert.equal(e.opens(), 1);
    assert.equal(e.paletteClosed(), true);
    assert.equal(e.attributes.get('aria-expanded'), 'true');
    assert.ok(e.classes.has('gex-quick-create-open'));
    e.binding.dispose();
});

test('backdrop click dismisses the menu while clicks inside sheet padding leave it open', () => {
    const e = environment();
    e.trigger.emit('click');
    e.panel.emit('click', { target: e.panel, clientX: 30, clientY: 220 });
    assert.equal(e.panel.open, true);
    e.panel.emit('click', { target: e.panel, clientX: 5, clientY: 100 });
    assert.equal(e.panel.open, false);
    assert.equal(e.classes.size, 0);
    e.binding.dispose();
});

test('choosing an action releases the native modal and scrolling before its destination opens', () => {
    const e = environment();
    e.trigger.emit('click');
    e.panel.emit('click', { target: { closest: () => ({}) } });
    assert.equal(e.panel.open, false);
    assert.equal(e.classes.size, 0);
    assert.equal(e.attributes.get('aria-expanded'), 'false');
    e.binding.dispose();
});

test('native Escape/close cleanup ignores an obsolete close event after the sheet has reopened', () => {
    const e = environment();
    e.trigger.emit('click');
    e.panel.open = false; e.panel.emit('close');
    assert.equal(e.classes.size, 0);
    e.trigger.emit('click');
    e.panel.emit('close');
    assert.equal(e.classes.size, 1);
    e.binding.dispose();
});

test('desktop cannot open the mobile menu; resizing to desktop dismisses an open sheet', () => {
    const e = environment(false);
    e.trigger.emit('click');
    assert.equal(e.opens(), 0);
    e.media.matches = true; e.trigger.emit('click');
    e.media.matches = false; e.media.emit('change');
    assert.equal(e.panel.open, false);
    assert.equal(e.classes.size, 0);
    e.binding.dispose();
});

test('disposing an open menu clears scroll lock and every listener', () => {
    const e = environment();
    e.trigger.emit('click'); e.binding.dispose();
    assert.equal(e.panel.open, false);
    assert.equal(e.classes.size, 0);
    for (const element of [e.trigger, e.panel, e.media]) assert.equal(element.listeners.size, 0);
});

test('mobile drawer hides duplicate invoice creation while desktop dock retains it', () => {
    const css = readFileSync(new URL('../../src/App/Server/GoldEx.Server/wwwroot/css/appearance.css', import.meta.url), 'utf8');
    const drawer = readFileSync(new URL('../../src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor', import.meta.url), 'utf8');
    const mobileStart = css.indexOf('@media (max-width: 959.98px)');
    const mobileEnd = css.indexOf('@media (max-width: 650px)', mobileStart);
    assert.ok(mobileStart >= 0 && mobileEnd > mobileStart);
    assert.match(css.slice(mobileStart, mobileEnd), /\.gex-sidebar \.gex-sidebar-create \{ display: none; \}/);
    assert.match(drawer, /class="gex-sidebar-create" href="@ClientRoutes\.Invoices\.Create"/);
    assert.match(css, /@media \(min-width: 960px\)/);
});
test('quick-create typography uses standard weights and readable action descriptions', () => {
    const css = readFileSync(new URL('../../src/App/Server/GoldEx.Server/wwwroot/css/appearance.css', import.meta.url), 'utf8');
    assert.match(css, /\.gex-quick-create-heading h2 \{[^}]*font-weight: 600;/);
    assert.match(css, /\.gex-quick-create-action strong \{[^}]*font-size: 14px;[^}]*font-weight: 600;/);
    assert.match(css, /\.gex-quick-create-action small \{[^}]*font-size: 12px;[^}]*font-weight: 400;[^}]*var\(--mud-palette-text-secondary\)/);
    assert.doesNotMatch(css, /\.gex-quick-create-(?:heading h2|action strong) \{[^}]*font-weight: (?:750|800);/);
});
