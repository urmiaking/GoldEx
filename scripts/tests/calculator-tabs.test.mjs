import { test } from 'node:test';
import assert from 'node:assert/strict';
import { bind } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/calculator-tabs.js';

function target() {
    const listeners = new Map();
    return { listeners, addEventListener: (type, fn) => listeners.set(type, fn), removeEventListener: type => listeners.delete(type),
        emit(type, event = {}) { if (type !== 'click' || !this.disabled) listeners.get(type)?.(event); } };
}
function environment({ width = 360, count = 6, rtl = true, reduced = false } = {}) {
    const classes = new Set(), frames = new Map(), scrolls = [], observers = [];
    let barWidth = width, offset = 0, frameId = 0;
    globalThis.document = { activeElement: null };
    globalThis.getComputedStyle = () => ({ direction: rtl ? 'rtl' : 'ltr' });
    globalThis.matchMedia = () => ({ matches: reduced });
    globalThis.requestAnimationFrame = fn => { frames.set(++frameId, fn); return frameId; };
    globalThis.cancelAnimationFrame = id => frames.delete(id);
    globalThis.ResizeObserver = class { constructor(fn) { this.fn = fn; observers.push(this); } observe() {} disconnect() { this.disconnected = true; } };
    globalThis.MutationObserver = globalThis.ResizeObserver;
    const left = target(), right = target();
    const bar = { get clientWidth() { return barWidth; }, classList: { toggle(name, on) { if (on) classes.add(name); else classes.delete(name); } },
        querySelector: selector => selector.includes('viewport') ? viewport : selector.includes('tablist') ? track : selector.includes('"left"') ? left : right };
    const viewport = { ...target(),
        get clientWidth() { return barWidth - (classes.has('has-overflow') ? 88 : 0); },
        getBoundingClientRect() { const start = classes.has('has-overflow') ? 44 : 0; return { left: start, right: start + this.clientWidth }; },
        scrollBy(options) {
            scrolls.push(options);
            const max = Math.max(0, track.scrollWidth - this.clientWidth);
            offset = rtl ? Math.max(-max, Math.min(0, offset + options.left)) : Math.min(max, Math.max(0, offset + options.left));
            this.emit('scroll');
        }
    };
    const items = Array.from({ length: count }, (_, index) => ({
        index, tabIndex: index ? -1 : 0, selected: index === 0,
        getAttribute(name) { return name === 'aria-selected' ? String(this.selected) : null; },
        closest() { return this; }, focus() { document.activeElement = this; },
        getBoundingClientRect() {
            const content = track.getBoundingClientRect();
            const left = rtl ? content.right - (index + 1) * 120 : content.left + index * 120;
            return { left, right: left + 120 };
        }
    }));
    const track = { ...target(),
        get scrollWidth() { return Math.max(count * 120, viewport.clientWidth); },
        getBoundingClientRect() {
            const view = viewport.getBoundingClientRect();
            const left = rtl ? view.right - this.scrollWidth - offset : view.left - offset;
            return { left, right: left + this.scrollWidth };
        },
        querySelectorAll: () => items, querySelector: () => items.find(x => x.selected), contains: item => items.includes(item)
    };
    const binding = bind({ querySelector: () => bar });
    const flush = () => { for (const [id, fn] of [...frames]) { frames.delete(id); fn(); } };
    return { binding, left, right, viewport, track, items, scrolls, frames, observers,
        offset: () => offset, flush,
        resize(value) { barWidth = value; observers[0].fn(); flush(); },
        select(index) { items.forEach(x => x.selected = x.index === index); observers[1].fn(); flush(); } };
}

test('RTL physical left arrow scrolls left; right arrow returns right without changing selection', () => {
    const e = environment();
    assert.equal(e.left.disabled, false); assert.equal(e.right.disabled, true);
    e.left.emit('click'); e.flush();
    assert.ok(e.scrolls.at(-1).left < 0); assert.ok(e.offset() < 0);
    assert.equal(e.right.disabled, false); assert.equal(e.items[0].selected, true);
    e.right.emit('click'); e.flush();
    assert.ok(e.scrolls.at(-1).left > 0); assert.equal(e.offset(), 0);
    assert.equal(e.right.disabled, true); e.binding.dispose();
});
test('LTR uses the same physical arrow directions and reaches both boundaries', () => {
    const e = environment({ rtl: false });
    assert.equal(e.left.disabled, true); assert.equal(e.right.disabled, false);
    for (let n = 0; n < 8; n++) { e.right.emit('click'); e.flush(); }
    assert.equal(e.right.disabled, true); assert.equal(e.left.disabled, false);
    for (let n = 0; n < 8; n++) { e.left.emit('click'); e.flush(); }
    assert.equal(e.offset(), 0); assert.equal(e.left.disabled, true); e.binding.dispose();
});
test('RTL boundaries disable only the exhausted direction, including after repeated clicks', () => {
    const e = environment();
    for (let n = 0; n < 8; n++) { e.left.emit('click'); e.flush(); }
    assert.equal(e.left.disabled, true); assert.equal(e.right.disabled, false);
    const end = e.offset(); e.left.emit('click'); e.flush(); assert.equal(e.offset(), end);
    e.binding.dispose();
});
test('keyboard follows RTL geometry, wraps, and leaves expensive panel activation to Enter/Space', () => {
    const e = environment(); let prevented = false;
    e.track.emit('keydown', { target: e.items[0], key: 'ArrowLeft', preventDefault() { prevented = true; } });
    assert.equal(prevented, true); assert.equal(document.activeElement, e.items[1]);
    assert.equal(e.items[0].selected, true); assert.equal(e.items[1].selected, false);
    e.track.emit('keydown', { target: e.items[1], key: 'End', preventDefault() {} });
    assert.equal(document.activeElement, e.items.at(-1));
    e.track.emit('keydown', { target: e.items.at(-1), key: 'ArrowLeft', preventDefault() {} });
    assert.equal(document.activeElement, e.items[0]);
    e.track.emit('focusout', { relatedTarget: null });
    assert.deepEqual(e.items.map(x => x.tabIndex), [0, -1, -1, -1, -1, -1]); e.binding.dispose();
});
test('selection reveals its tab using geometry without scrolling the document vertically', () => {
    const e = environment(); e.select(5);
    assert.ok(e.offset() < 0); assert.equal(e.items[5].tabIndex, 0);
    assert.ok(e.scrolls.every(x => !('top' in x))); e.binding.dispose();
});
test('desktop/guest tabs do not reserve arrow space and shrinking reveals the active tab', () => {
    const e = environment({ width: 1000 });
    assert.equal(e.left.hidden, true); assert.equal(e.right.hidden, true);
    e.select(5); e.resize(360);
    assert.equal(e.left.hidden, false); assert.ok(e.offset() < 0); e.binding.dispose();
    const guest = environment({ count: 1 }); assert.equal(guest.left.hidden, true); guest.binding.dispose();
});
test('reduced motion and disposal release every observer, listener and animation frame', () => {
    const e = environment({ reduced: true }); e.left.emit('click');
    assert.equal(e.scrolls.at(-1).behavior, 'auto');
    e.binding.dispose(); assert.equal(e.frames.size, 0);
    for (const element of [e.left, e.right, e.track, e.viewport]) assert.equal(element.listeners.size, 0);
    assert.ok(e.observers.every(x => x.disconnected));
});
