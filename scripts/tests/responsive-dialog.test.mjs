import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';

const source = readFileSync(new URL('../../src/App/Client/GoldEx.Client.Components/wwwroot/js/responsive-dialog.js', import.meta.url), 'utf8');
function harness() {
    const classes = new Set(), viewportEvents = new Map(), windowEvents = new Map();
    const viewport = { height: 740, offsetTop: 0, addEventListener: (key, fn) => viewportEvents.set(fn, key), removeEventListener: (key, fn) => viewportEvents.delete(fn) };
    const document = { activeElement: null, documentElement: { style: { overflow: 'auto', overscrollBehavior: 'contain' }, classList: { add: key => classes.add(key), remove: key => classes.delete(key) } } };
    const window = { visualViewport: viewport, innerHeight: 740, addEventListener: (key, fn) => windowEvents.set(fn, key), removeEventListener: (key, fn) => windowEvents.delete(fn) };
    const context = { document, window };
    vm.runInNewContext(source.replaceAll('export function ', 'function ') + '\nthis.api = { activate, deactivate, canDismiss, setOpen, focusSession };', context);
    function button() { return { isConnected: true, closest: () => null, getClientRects: () => [{}], focus() { document.activeElement = this; } }; }
    function dialog() {
        const events = new Map(), props = new Map(), controls = [button(), button()];
        return { events, props, controls, popovers: [], open: false, shows: 0,
            style: { setProperty: (key, value) => props.set(key, value) },
            querySelectorAll(selector) { return selector.startsWith('.mud-popover') ? this.popovers : controls; },
            addEventListener: (key, fn) => events.set(key, fn), removeEventListener: key => events.delete(key),
            querySelector: () => controls[0],
            show() { this.open = true; },
            showModal() { this.open = true; this.shows++; }, close() { this.open = false; }, focus() { document.activeElement = this; }
        };
    }
    return { ...context.api, document, window, viewport, classes, viewportEvents, windowEvents, button, dialog };
}

test('native modality, idempotence, Escape prevention and focus restoration', () => {
    const host = harness(), modal = host.dialog(), trigger = host.button();
    trigger.focus();
    host.activate(modal); host.activate(modal);
    assert.equal(modal.open, true); assert.equal(modal.shows, 1);
    assert.equal(host.document.activeElement, modal.controls[0]);
    let prevented = false;
    modal.events.get('cancel')({ preventDefault() { prevented = true; } });
    assert.equal(prevented, true);
    host.deactivate(modal); host.deactivate(modal);
    assert.equal(modal.open, false); assert.equal(host.document.activeElement, trigger);
    assert.equal(modal.events.size, 0);
});

test('keyboard Tab and Shift+Tab wrap within the active dialog', () => {
    const host = harness(), modal = host.dialog();
    host.activate(modal);
    let prevented = 0;
    modal.events.get('keydown')({ key: 'Tab', shiftKey: true, preventDefault() { prevented++; } });
    assert.equal(host.document.activeElement, modal.controls[1]);
    modal.events.get('keydown')({ key: 'Tab', shiftKey: false, preventDefault() { prevented++; } });
    assert.equal(host.document.activeElement, modal.controls[0]);
    assert.equal(prevented, 2);
});

test('nested dialogs preserve scroll lock and dismiss only the top window after popovers close', () => {
    const host = harness(), parent = host.dialog(), child = host.dialog();
    host.activate(parent); host.activate(child);
    assert.equal(host.document.documentElement.style.overflow, 'hidden');
    assert.equal(host.canDismiss(parent), false);
    child.popovers = [{ getClientRects: () => [{}] }];
    assert.equal(host.canDismiss(child), false);
    child.popovers = [];
    assert.equal(host.canDismiss(child), true);
    host.deactivate(child);
    assert.equal(host.document.documentElement.style.overflow, 'hidden');
    assert.equal(host.canDismiss(parent), true);
    host.deactivate(parent);
    assert.equal(host.classes.size, 0);
    assert.equal(host.document.documentElement.style.overflow, 'auto');
    assert.equal(host.document.documentElement.style.overscrollBehavior, 'contain');
});

test('visual viewport tracks keyboard size/offset and disposal releases all listeners', () => {
    const host = harness(), modal = host.dialog();
    host.activate(modal);
    host.viewport.height = 410; host.viewport.offsetTop = 25;
    for (const fn of host.viewportEvents.keys()) fn();
    assert.equal(modal.props.get('--gex-modal-viewport-height'), '410px');
    assert.equal(modal.props.get('--gex-modal-viewport-offset'), '25px');
    host.deactivate(modal);
    assert.equal(host.viewportEvents.size, 0);
    assert.equal(host.windowEvents.size, 0);
});

test('persistent overlay host enters/leaves modality without replacing provider ancestors', () => {
    const host = harness(), modal = host.dialog();
    host.setOpen(modal, false);
    assert.equal(modal.open, true, 'Inactive host stays nonmodal for normal page popovers');
    assert.equal(host.classes.size, 0);
    host.setOpen(modal, true);
    host.focusSession(modal, 'first');
    assert.equal(modal.shows, 1);
    host.setOpen(modal, false);
    assert.equal(modal.open, true);
    assert.equal(host.classes.size, 0);
    assert.equal(host.document.documentElement.style.overflow, 'auto');
    host.setOpen(modal, true);
    assert.equal(modal.shows, 2);
    host.deactivate(modal);
});

test('returning from a child session restores the parent form control', () => {
    const host = harness(), modal = host.dialog();
    host.activate(modal);
    const original = modal.controls[1];
    original.closest = () => ({ getAttribute: () => 'parent' });
    modal.events.get('focusin')({ target: original });
    host.focusSession(modal, 'child');
    assert.equal(host.document.activeElement, modal.controls[0]);
    host.focusSession(modal, 'parent');
    assert.equal(host.document.activeElement, original);
    host.deactivate(modal);
});
