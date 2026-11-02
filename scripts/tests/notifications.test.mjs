import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';
import { bind, getState, subscribe, disable } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/notifications.js';

function browser() {
    const storage = new Map(); let current = null, subscriptions = 0, permissions = 0, unsubscriptions = 0;
    globalThis.window = { isSecureContext: true, PushManager: {}, Notification: {} };
    globalThis.Notification = { permission: 'default', async requestPermission() { permissions++; this.permission = 'granted'; return this.permission; } };
    globalThis.localStorage = { getItem: key => storage.get(key), setItem: (key, value) => storage.set(key, value), removeItem: key => storage.delete(key) };
    const worker = { pushManager: {
        async getSubscription() { return current; },
        async subscribe(options) {
            subscriptions++;
            current = { endpoint: 'https://fcm.googleapis.com/test', options,
                toJSON: () => ({ endpoint: 'https://fcm.googleapis.com/test', keys: { p256dh: 'curve', auth: 'auth' } }),
                async unsubscribe() { unsubscriptions++; current = null; return true; } };
            return current;
        }
    }, async getNotifications() { return []; } };
    Object.defineProperty(globalThis, 'navigator', { configurable: true, value: { serviceWorker: {
        ready: Promise.resolve(worker), register: async () => worker, getRegistration: async () => worker
    } } });
    return { storage, counts: () => ({ subscriptions, permissions, unsubscriptions }) };
}
test('registration requests permission only from the explicit enable action and reuses the subscription', async () => {
    const e = browser();
    assert.equal((await subscribe('BAAA', false)).endpoint, null);
    assert.equal(e.counts().permissions, 0);
    assert.equal((await subscribe('BAAA', true)).endpoint, 'https://fcm.googleapis.com/test');
    await subscribe('BAAA', false);
    assert.deepEqual(e.counts(), { subscriptions: 1, permissions: 1, unsubscriptions: 0 });
    assert.equal((await getState()).subscribed, true);
});
test('disabling stops browser delivery and opt-out survives login/restoration', async () => {
    const e = browser(); await subscribe('BAAA', true); await disable(); await subscribe('BAAA', false);
    assert.equal((await getState()).subscribed, false);
    assert.deepEqual(e.counts(), { subscriptions: 1, permissions: 1, unsubscriptions: 1 });
    await subscribe('BAAA', true);
    assert.equal(e.counts().subscriptions, 2);
});
test('a new VAPID key replaces the old browser subscription', async () => {
    const e = browser(); await subscribe('BAAA', true); await subscribe('BAAB', false);
    assert.deepEqual(e.counts(), { subscriptions: 2, permissions: 1, unsubscriptions: 1 });
});
test('unsupported and denied browsers do not attempt to register devices', async () => {
    const e = browser(); Notification.requestPermission = async () => 'denied';
    assert.equal((await subscribe('BAAA', true)).permission, 'denied');
    assert.equal(e.counts().subscriptions, 0);
    delete window.PushManager;
    assert.equal((await getState()).supported, false);
});

function panel() {
    const listeners = new Map(), classes = new Set(); let focused = 0, paletteClosed = 0;
    globalThis.document = { activeElement: { isConnected: true, focus() { focused++; } },
        documentElement: { classList: { add: x => classes.add(x), remove: x => classes.delete(x) } },
        querySelectorAll: () => [{ hidePopover() { paletteClosed++; } }] };
    const element = { open: false, addEventListener: (type, fn) => listeners.set(type, fn), removeEventListener: type => listeners.delete(type),
        showModal() { this.open = true; }, close() { this.open = false; }, closest: () => null,
        getBoundingClientRect: () => ({ left: 10, right: 600, top: 20, bottom: 700 }) };
    return { element, binding: bind(element), classes, listeners, counts: () => ({ focused, paletteClosed }) };
}
test('native notification modal releases scroll lock and restores focus after Escape or disposal', () => {
    const e = panel(); e.binding.open();
    assert.equal(e.element.open, true); assert.equal(e.counts().paletteClosed, 1);
    e.element.open = false; e.listeners.get('close')();
    assert.equal(e.classes.size, 0); assert.equal(e.counts().focused, 1);
    e.binding.open(); e.binding.dispose();
    assert.equal(e.element.open, false); assert.equal(e.classes.size, 0); assert.equal(e.listeners.size, 0);
});
test('padding clicks preserve the dialog while backdrop and close-button clicks dismiss it', () => {
    const e = panel(); e.binding.open();
    e.listeners.get('click')({ target: e.element, clientX: 50, clientY: 50 });
    assert.equal(e.element.open, true);
    e.listeners.get('click')({ target: e.element, clientX: 1, clientY: 1 });
    assert.equal(e.element.open, false);
    e.binding.open(); e.listeners.get('click')({ target: { closest: () => ({}) } });
    assert.equal(e.element.open, false); e.binding.dispose();
});

function worker() {
    const listeners = new Map(), shown = [], opened = [];
    const self = { location: { origin: 'https://goldex.example' }, addEventListener: (type, fn) => listeners.set(type, fn),
        registration: { async showNotification(title, options) { shown.push({ title, ...options }); } },
        clients: { async matchAll() { return []; }, async openWindow(url) { opened.push(url); } } };
    vm.runInNewContext(readFileSync(new URL('../../src/App/Client/GoldEx.Client/wwwroot/notification-worker.js', import.meta.url), 'utf8'), { self, URL });
    return { listeners, shown, opened };
}
test('malformed payload still shows a generic notification, and external deep links are rejected', async () => {
    const e = worker(); let completion;
    e.listeners.get('push')({ data: { json() { throw new Error(); } }, waitUntil: value => completion = value }); await completion;
    assert.equal(e.shown[0].title, 'GoldEx');
    e.listeners.get('push')({ data: { json: () => ({ title: 'Reminder', url: 'https://evil.example/' }) }, waitUntil: value => completion = value }); await completion;
    assert.equal(e.shown[1].data.url, '/');
});
test('notification click opens the same-origin invoice destination', async () => {
    const e = worker(); let completion, closed = false;
    e.listeners.get('notificationclick')({ notification: { close() { closed = true; }, data: { url: '/invoices/one?tab=0' } }, waitUntil: value => completion = value });
    await completion; assert.equal(closed, true); assert.deepEqual(e.opened, ['https://goldex.example/invoices/one?tab=0']);
});
