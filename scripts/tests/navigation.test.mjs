import { test } from 'node:test';
import assert from 'node:assert/strict';
import { bind } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/navigation.js';

function environment(mobile = true, paletteOpen = false) {
    const listeners = new Map(), attributes = new Map(), classes = new Set();
    const background = [{ inert: false }, { inert: true }];
    let paletteClosed = false;
    const palette = { hidePopover() { paletteClosed = true; } };
    const media = { matches: mobile, addEventListener: (_, fn) => listeners.set('resize', fn), removeEventListener: () => listeners.delete('resize') };
    globalThis.matchMedia = () => media;
    globalThis.document = { documentElement: { classList: { add: key => classes.add(key), remove: key => classes.delete(key) } } };
    const control = () => ({ isConnected: true, getClientRects: () => [1], closest: () => null, focus() { document.activeElement = this; } });
    const trigger = control(), first = control(), last = control();
    document.activeElement = trigger;
    const panel = {
        closest: () => ({ querySelectorAll: selector => selector === '.gex-palette-sheet:popover-open' ? (paletteOpen ? [palette] : []) : background }),
        querySelectorAll: () => [first, last], contains: element => [first, last].includes(element),
        setAttribute: (key, value) => attributes.set(key, value), removeAttribute: key => attributes.delete(key),
        addEventListener: (_, fn) => listeners.set('keydown', fn), removeEventListener: () => listeners.delete('keydown'),
        focus() { document.activeElement = this; }
    };
    return { menu: bind(panel), media, listeners, background, classes, attributes, trigger, first, last, paletteClosed: () => paletteClosed };
}

test('mobile menu contains focus and restores each background element’s original inert state', () => {
    const e = environment();
    e.menu.update(true);
    assert.equal(document.activeElement, e.first);
    assert.deepEqual(e.background.map(x => x.inert), [true, true]);
    assert.equal(e.attributes.get('aria-modal'), 'true');
    assert.ok(e.classes.has('gex-navigation-open'));
    e.menu.update(false);
    assert.deepEqual(e.background.map(x => x.inert), [false, true]);
    assert.equal(document.activeElement, e.trigger);
    assert.equal(e.attributes.has('aria-modal'), false);
    assert.equal(e.classes.size, 0);
    e.menu.dispose();
});

test('Tab and Shift+Tab wrap at menu boundaries without changing interior focus on rerender', () => {
    const e = environment();
    e.menu.update(true);
    let prevented = 0;
    const tab = shiftKey => e.listeners.get('keydown')({ key: 'Tab', shiftKey, preventDefault: () => prevented++ });
    tab(true);
    assert.equal(document.activeElement, e.last);
    e.menu.update(true);
    assert.equal(document.activeElement, e.last);
    tab(false);
    assert.equal(document.activeElement, e.first);
    assert.equal(prevented, 2);
    e.menu.dispose();
});

test('desktop navigation never locks the page; crossing the breakpoint releases the mobile modal', () => {
    const e = environment(false);
    e.menu.update(true);
    assert.equal(e.classes.size, 0);
    assert.deepEqual(e.background.map(x => x.inert), [false, true]);
    e.media.matches = true;
    e.listeners.get('resize')();
    assert.equal(e.attributes.get('role'), 'dialog');
    e.media.matches = false;
    e.listeners.get('resize')();
    assert.equal(e.attributes.get('role'), 'complementary');
    assert.deepEqual(e.background.map(x => x.inert), [false, true]);
    e.menu.dispose();
});

test('disposing an open navigation restores scrolling, inert state and event listeners', () => {
    const e = environment();
    e.menu.update(true);
    e.menu.dispose();
    assert.equal(e.classes.size, 0);
    assert.deepEqual(e.background.map(x => x.inert), [false, true]);
    assert.equal(e.listeners.size, 0);
});

test('opening navigation closes an already open top-layer palette before locking the background', () => {
    const e = environment(true, true);
    e.menu.update(true);
    assert.equal(e.paletteClosed(), true);
    assert.equal(document.activeElement, e.first);
    e.menu.dispose();
});
