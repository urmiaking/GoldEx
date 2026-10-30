import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { runInNewContext } from 'node:vm';

const read = path => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');
const source = read('src/App/Client/GoldEx.Client/wwwroot/js/reporting-customization.js');
const style = read('src/App/Server/GoldEx.Server/wwwroot/css/devexpress-viewer-responsive.css');
const app = read('src/App/Server/GoldEx.Server/Components/App.razor');
const viewer = read('src/App/Client/GoldEx.Client/Pages/Invoices/Viewer.razor');

function instantiate(width) {
    const registered = new Map();
    const doc = {
        head: { appendChild: (x) => registered.set(x.id, x) },
        getElementById: id => registered.get(id),
        createElement: tag => ({ tag, id: '', type: '', innerHTML: '' })
    };
    const win = {
        innerWidth: width,
        DevExpress: { Reporting: { Viewer: { ActionId: {
            PrintPage: 1, Search: 2, ExportTo: 3, ZoomIn: 4, ZoomOut: 5, FullScreen: 6
        } } } }
    };
    runInNewContext(source, { window: win, document: doc });
    return { app: win.reportingCustomization, registered, win };
}

test('before-render fits page width on mobile and retains whole page on desktop', () => {
    for (const [width, zoom] of [[375, -1], [768, -1], [959, -1], [960, 0], [1366, 0]]) {
        const {app} = instantiate(width);
        const event = {reportPreview: {zoom: 1}};
        app.onBeforeRender(null, event);
        assert.equal(event.reportPreview.zoom, zoom, 'viewport ' + width);
    }
});

test('native toolbar actions keep execute callbacks while receiving original GoldEx SVG icons', () => {
    const {app, registered} = instantiate(375);
    const actions = Array.from({length: 6}, (_, i) => ({id: i+1, execute: () => 'native ' + i}));
    const original = new Map(actions.map(a => [a.id, a.execute]));
    const args = { GetById: id => actions.find(a => a.id === id), Actions: actions };
    app.onCustomizeMenuActions(null, args);
    for (let i=0; i<actions.length; i++) {
        assert.equal(actions[i].execute, original.get(actions[i].id));
        assert.match(actions[i].imageTemplateName, /^gex-report-icon-/);
    }
    assert.ok(registered.size >= 6);
    for (const template of registered.values()) {
        assert.equal(template.type, 'text/html');
        assert.match(template.innerHTML, /<svg class="gex-report-glyph"/);
        assert.match(template.innerHTML, /stroke="currentColor"/);
    }
    app.onCustomizeMenuActions(null, args);
    assert.equal(registered.size, 8, 'SVG templates should not be duplicated');
});

test('toolbar customization remains safe if optional DevExpress actions are missing', () => {
    const {app, win} = instantiate(390);
    delete win.DevExpress;
    assert.doesNotThrow(() => app.onCustomizeMenuActions(null, {GetById() {return null;}}));
    assert.doesNotThrow(() => app.onCustomizeMenuActions(null, null));
    assert.doesNotThrow(() => app.onBeforeRender(null, null));
});

test('mobile toolbar is horizontally scrollable, never clips hidden actions', () => {
    assert.match(style, /@media \(max-width: 959\.98px\)/);
    assert.match(style, /\.dxrd-toolbar-wrapper[\s\S]*?overflow-x: auto !important/);
    assert.match(style, /width: max-content !important/);
    assert.match(style, /touch-action: pan-x pan-y/);
    assert.match(style, /-webkit-overflow-scrolling: touch/);
    assert.match(viewer, /gex-report-mobile-hint/);
    assert.match(style, /--gex-report-ink/);
    assert.match(style, /-webkit-text-fill-color: var\(--gex-report-ink\)/);
    assert.match(style, /\.dxrd-preview-wrapper/);
    assert.match(app, /css\/devexpress-viewer-responsive\.css/);
    assert.ok(app.indexOf('css/devexpress-viewer-responsive.css') > app.indexOf('css/devexpress-goldex.css'));
});