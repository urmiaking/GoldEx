import { test } from 'node:test';
import assert from 'node:assert/strict';
import { bind } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/palette-picker.js';

function target() {
    const listeners = new Map();
    return {
        listeners,
        addEventListener(type, fn) { if (!listeners.has(type)) listeners.set(type, new Set()); listeners.get(type).add(fn); },
        removeEventListener(type, fn) { const set = listeners.get(type); set?.delete(fn); if (!set?.size) listeners.delete(type); },
        emit(type, values = {}) {
            const event = { type, cancelable: true, prevented: false, stopped: false,
                preventDefault() { this.prevented = true; }, stopImmediatePropagation() { this.stopped = true; }, ...values };
            for (const listener of listeners.get(type) ?? []) listener(event);
            return event;
        }
    };
}

function environment({ saved = null, blocked = false } = {}) {
    const storage = new Map(saved === null ? [] : [['GoldEx.PickerPosition', saved]]);
    globalThis.localStorage = { getItem(key) { if (blocked) throw Error('blocked'); return storage.get(key) ?? null; },
        setItem(key, value) { if (blocked) throw Error('blocked'); storage.set(key, value); } };
    globalThis.innerWidth = 1440;
    globalThis.innerHeight = 900;
    const header = { bottom: 82 }, nav = { height: 0, top: 900 };
    globalThis.window = target();
    window.visualViewport = Object.assign(target(), { offsetTop: 0, width: 1440, height: 900 });
    globalThis.document = Object.assign(target(), { documentElement: { clientWidth: 1440 },
        querySelector: selector => ({ getBoundingClientRect: () => selector === '.gex-topbar' ? header : nav }) });
    globalThis.requestAnimationFrame = fn => { fn(); return 1; };
    globalThis.cancelAnimationFrame = () => {};
    const host = { style: {}, dataset: {}, getBoundingClientRect: () => ({ top: Number.parseFloat(host.style.top) || 0 }) };
    const attributes = new Map(), captures = new Set();
    const trigger = Object.assign(target(), {
        isConnected: true, offsetHeight: 56, closest: () => host,
        getBoundingClientRect: () => ({ top: Number.parseFloat(host.style.top), height: 56 }),
        setAttribute: (key, value) => attributes.set(key, value),
        setPointerCapture: id => captures.add(id), hasPointerCapture: id => captures.has(id), releasePointerCapture: id => captures.delete(id),
        click() { if (!this.emit('click', { detail: 0 }).prevented) popover.open(); }
    });
    const popover = Object.assign(target(), {
        opened: false, offsetWidth: 354, offsetHeight: 380, style: { setProperty() {} },
        matches() { return this.opened; }, querySelector: () => ({ setAttribute() {} }),
        open() { this.opened = true; this.emit('beforetoggle', { newState: 'open' }); this.emit('toggle'); },
        hidePopover() { this.opened = false; this.emit('toggle'); }
    });
    return { binding: bind(trigger, popover), trigger, popover, host, storage, attributes, captures, header, nav,
        top: () => Number.parseFloat(host.style.top) };
}

test('keyboard movement clamps between header and bottom navigation and persists the position', () => {
    const e = environment();
    e.trigger.emit('keydown', { key: 'Home' });
    assert.equal(e.top(), 94);
    e.trigger.emit('keydown', { key: 'ArrowDown' });
    assert.equal(e.top(), 118);
    e.trigger.emit('keydown', { key: 'End' });
    assert.equal(e.top(), 832);
    assert.equal(e.storage.get('GoldEx.PickerPosition'), '1');
    e.nav.height = 71; e.nav.top = 729; e.header.bottom = 70;
    window.visualViewport.height = 800;
    window.emit('resize');
    assert.equal(e.top(), 661);
    e.binding.dispose();
});

test('invalid saved positions and blocked storage do not prevent using the picker', () => {
    for (const options of [{ saved: 'invalid' }, { saved: '' }, { blocked: true }]) {
        const e = environment(options);
        assert.equal(e.top(), 463);
        assert.doesNotThrow(() => e.trigger.emit('keydown', { key: 'ArrowUp' }));
        e.binding.dispose();
    }
});

test('mouse drag closes the sheet, saves the new position and suppresses only the trailing click', () => {
    const e = environment();
    e.popover.open();
    e.trigger.emit('pointerdown', { pointerType: 'mouse', isPrimary: true, button: 0, pointerId: 1, clientY: 480, clientX: 10 });
    e.trigger.emit('pointermove', { pointerId: 1, clientY: 580, clientX: 10 });
    assert.equal(e.top(), 563);
    assert.equal(e.popover.opened, false);
    e.trigger.emit('pointerup', { pointerId: 1 });
    assert.equal(e.captures.size, 0);
    assert.ok(e.storage.has('GoldEx.PickerPosition'));
    assert.equal(e.trigger.emit('click', { detail: 1 }).prevented, true);
    assert.equal(e.trigger.emit('click', { detail: 1 }).prevented, false);
    e.binding.dispose();
});

test('canceling a drag restores the prior position without overwriting the saved preference', () => {
    const e = environment({ saved: '.25' });
    const original = e.top();
    e.trigger.emit('pointerdown', { pointerType: 'pen', isPrimary: true, button: 0, pointerId: 2, clientY: 300, clientX: 10 });
    e.trigger.emit('pointermove', { pointerId: 2, clientY: 400, clientX: 10 });
    e.trigger.emit('pointercancel', { pointerId: 2 });
    assert.equal(e.top(), original);
    assert.equal(e.storage.get('GoldEx.PickerPosition'), '.25');
    e.binding.dispose();
});

test('touch tap opens the sheet once and touch dragging does not scroll the page', () => {
    const e = environment();
    const point = { identifier: 4, clientX: 10, clientY: 400 };
    assert.equal(e.trigger.emit('touchstart', { touches: [point], changedTouches: [point] }).prevented, true);
    document.emit('touchend', { changedTouches: [point] });
    assert.equal(e.popover.opened, true);
    assert.equal(e.attributes.get('aria-expanded'), 'true');
    assert.equal(e.trigger.emit('click', { detail: 1 }).prevented, true);
    e.trigger.emit('touchstart', { touches: [point], changedTouches: [point] });
    const moved = { ...point, clientY: 480 };
    assert.equal(document.emit('touchmove', { changedTouches: [moved] }).prevented, true);
    document.emit('touchend', { changedTouches: [moved] });
    assert.equal(e.popover.opened, false);
    assert.equal(e.top(), 543);
    assert.equal(document.listeners.size, 0);
    e.binding.dispose();
});

test('disposing during a touch gesture closes the sheet and removes every event listener', () => {
    const e = environment();
    const point = { identifier: 1, clientX: 10, clientY: 400 };
    e.trigger.emit('touchstart', { touches: [point], changedTouches: [point] });
    e.popover.open();
    e.binding.dispose();
    assert.equal(e.popover.opened, false);
    for (const element of [e.trigger, e.popover, document, window, window.visualViewport]) assert.equal(element.listeners.size, 0);
});
