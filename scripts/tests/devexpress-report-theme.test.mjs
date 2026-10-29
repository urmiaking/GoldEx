import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const app=read('src/App/Server/GoldEx.Server/Components/App.razor');
const viewer=read('src/App/Client/GoldEx.Client/Pages/Invoices/Viewer.razor');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/devexpress-goldex.css');
const callback=read('src/App/Client/GoldEx.Client/wwwroot/js/reporting-customization.js');

test('report theme loads after existing appearance CSS',()=>{
 assert.ok(app.indexOf('css/devexpress-goldex.css')>app.indexOf('css/appearance.css'));
 assert.match(app, /href="@Assets\["css\/devexpress-goldex\.css"\]"/);
});
test('invoice viewer is scoped but preserves DevExpress preview, export and existing callbacks',()=>{
 assert.match(viewer,/class="gex-report-viewer"/);
 assert.match(viewer,/DxWasmDocumentViewer ReportName="@_reportUrl"/);
 assert.match(viewer,/DxDocumentViewerExportSettings UseSameTab=true/);
 assert.match(viewer,/DxWasmDocumentViewerRequestOptions InvokeAction="DXXRDV"/);
 assert.match(viewer,/DxDocumentViewerCallbacks BeforeRender="reportingCustomization.onBeforeRender"/);
 assert.match(callback,/args\.reportPreview\.zoom = window\.innerWidth < 960 \? -1 : 0/);
});
test('documented DevExpress Reporting dxd palette is styled only within viewer wrapper',()=>{
 for(const name of ['dxd-back-primary','dxd-back-secondary','dxd-border-primary','dxd-border-accented','dxd-icon-fill','dxd-text-primary','dxd-side-panel-tabs-back-color'])
  assert.ok(css.includes('.'+name),name);
 assert.doesNotMatch(css,/^\s*\.dx-blazor-reporting\s+[.#]/m);
 assert.doesNotMatch(css,/^\s*\.dx-designer-viewport\s+[.#]/m);
 assert.match(css,/--gex-report-accent: var\(--mud-palette-primary/);
 assert.match(css,/--gex-report-surface: var\(--mud-palette-surface/);
});
test('toolbar, side panel and search are customized without touching printed page or clipping export menus',()=>{
 for(const name of ['dxrd-toolbar-wrapper','dxrd-toolbar-item','dxrd-tab-panel','dxrd-preview-search-wrapper','dxrd-preview-surface','dx-texteditor'])
  assert.ok(css.includes('.'+name),name);
 assert.match(css,/backdrop-filter: blur\(14px\)/);
 assert.match(css,/@media \(max-width: 959\.98px\)/);
 assert.match(css,/@media \(prefers-reduced-motion: reduce\)/);
 assert.doesNotMatch(css,/\.dxrd-report-preview-brick\s*\{/);
 assert.doesNotMatch(css,/overflow:\s*hidden/);
});
