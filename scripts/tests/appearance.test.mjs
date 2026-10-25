import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { change, sync } from '../../src/App/Client/GoldEx.Client.Components/wwwroot/js/appearance.js';

test('palette display names remain valid Persian Unicode in the actual C# source', () => {
    const source = readFileSync(new URL('../../src/App/Client/GoldEx.Client.Components/Themes/ColorPalettes.cs', import.meta.url), 'utf8');
    const options = source.slice(source.indexOf('Options { get; }'), source.indexOf('public static readonly'));
    const names = [...options.matchAll(/\("[A-Za-z]+",\s*"([^"]+)"\)/g)].map(match => match[1]);
    assert.equal(names.length, 6);
    for (const name of names) assert.match(name, /^[\p{Script=Arabic}\s\u200c]+$/u);
});

function environment({ reduced = false, supported = true, rejectReady = false, rejectStart = false, rejectAnimation = false } = {}) {
    const log = [];
    const root = {
        dataset: { gexMode: 'light', gexPalette: 'Gold' }, style: {},
        animate(frames, options) {
            log.push({ type: 'animate', frames, options });
            return { finished: rejectAnimation ? Promise.reject(new Error('animation rejected')) : Promise.resolve(),
                cancel() { log.push('cancel-animation'); } };
        }
    };
    globalThis.document = {
        documentElement: root, visibilityState: 'visible',
        querySelector: () => ({ setAttribute: (...args) => log.push(args) })
    };
    globalThis.getComputedStyle = () => ({ getPropertyValue: () => '#fafaf8' });
    globalThis.matchMedia = () => ({ matches: reduced });
    globalThis.innerWidth = 1440;
    globalThis.innerHeight = 900;
    if (supported) {
        document.startViewTransition = apply => {
            if (rejectStart) throw new Error('snapshot unavailable');
            const updated = Promise.resolve().then(apply);
            return {
                ready: updated.then(() => { if (rejectReady) throw new Error('ready rejected'); }),
                updateCallbackDone: updated,
                finished: updated.then(() => { log.push('snapshots-removed'); }),
                skipTransition() { log.push('skip'); }
            };
        };
    }
    const receiver = {
        async invokeMethodAsync(name, dark, palette) {
            log.push({ type: 'apply', name, dark, palette });
            await Promise.resolve(); // Model the awaited Blazor render.
            log.push('rendered');
        }
    };
    const source = { isConnected: true, getBoundingClientRect: () => ({ left: 100, top: 20, width: 44, height: 44 }) };
    return { log, root, receiver, source };
}

test('dark reveal captures the new render and retains its clip until snapshots disappear', async () => {
    const e = environment();
    await change(e.receiver, true, 'Gold', e.source);
    const animation = e.log.find(v => v.type === 'animate');
    assert.equal(animation.options.pseudoElement, '::view-transition-new(root)');
    assert.equal(animation.options.fill, 'both');
    assert.equal(animation.options.duration, 800);
    assert.match(animation.frames.clipPath[0], /^circle\(0px at 122px 42px\)/);
    assert.ok(e.log.indexOf('rendered') < e.log.indexOf(animation));
    assert.ok(e.log.indexOf('snapshots-removed') < e.log.indexOf('cancel-animation'));
    assert.equal(e.root.dataset.gexMode, 'dark');
    assert.equal(e.root.dataset.gexAppearanceTransition, undefined);
});

test('returning to light contracts the old dark snapshot', async () => {
    const e = environment();
    e.root.dataset.gexMode = 'dark';
    await change(e.receiver, false, 'Gold', e.source);
    const animation = e.log.find(v => v.type === 'animate');
    assert.equal(animation.options.pseudoElement, '::view-transition-old(root)');
    assert.match(animation.frames.clipPath[1], /^circle\(0px/);
});

test('changing palette in dark mode expands the new palette and preserves mode', async () => {
    const e = environment();
    e.root.dataset.gexMode = 'dark';
    await change(e.receiver, true, 'Blue', e.source);
    assert.equal(e.root.dataset.gexMode, 'dark');
    assert.equal(e.root.dataset.gexPalette, 'Blue');
    assert.equal(e.log.find(v => v.type === 'animate').options.pseudoElement, '::view-transition-new(root)');
});

for (const [name, options] of [
    ['reduced motion', { reduced: true }],
    ['unsupported snapshots', { supported: false }],
    ['rejected snapshot readiness', { rejectReady: true }],
    ['synchronous snapshot failure', { rejectStart: true }],
    ['interrupted animation', { rejectAnimation: true }]
]) {
    test(name + ' still applies the new appearance exactly once', async () => {
        const e = environment(options);
        await change(e.receiver, true, 'Purple', e.source);
        assert.equal(e.log.filter(v => v.type === 'apply').length, 1);
        assert.equal(e.root.dataset.gexPalette, 'Purple');
        assert.equal(e.root.dataset.gexAppearanceTransition, undefined);
    });
}

test('hidden documents and detached triggers change immediately', async () => {
    const e = environment();
    document.visibilityState = 'hidden';
    await change(e.receiver, true, 'Green', e.source);
    assert.equal(e.log.filter(v => v.type === 'animate').length, 0);
    document.visibilityState = 'visible';
    e.source.isConnected = false;
    await change(e.receiver, false, 'Green', e.source);
    assert.equal(e.root.dataset.gexMode, 'light');
});

test('initial sync updates browser appearance without animating', () => {
    const e = environment();
    sync(true, 'Orange');
    assert.equal(e.root.style.colorScheme, 'dark');
    assert.equal(e.root.dataset.gexPalette, 'Orange');
    assert.equal(e.log.filter(v => v.type === 'animate').length, 0);
});
