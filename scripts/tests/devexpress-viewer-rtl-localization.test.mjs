import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {runInNewContext} from 'node:vm';

const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const script=read('src/App/Client/GoldEx.Client/wwwroot/js/reporting-customization.js');
const viewer=read('src/App/Client/GoldEx.Client/Pages/Invoices/Viewer.razor');
const style=read('src/App/Server/GoldEx.Server/wwwroot/css/devexpress-viewer-fixes.css');

function load(){
 const registered=new Map();
 const doc={head:{appendChild:e=>registered.set(e.id,e)},getElementById:id=>registered.get(id),
   createElement:()=>({})};
 const ids={PrevPage:'prev',NextPage:'next'};
 const actions=new Map([['prev',{clickAction:()=>1}],['next',{clickAction:()=>2}]]);
 const win={innerWidth:1360,DevExpress:{Reporting:{Viewer:{ActionId:ids}}}};
 runInNewContext(script,{window:win,document:doc});
 return {cb:win.reportingCustomization,doc,registered,actions};
}
test('RTL Previous icon points right and Next icon points left, without swapping callbacks',()=>{
 const {cb,actions,registered}=load();
 const prev=actions.get('prev').clickAction,next=actions.get('next').clickAction;
 cb.onCustomizeMenuActions(null,{GetById:key=>actions.get(key)});
 assert.equal(actions.get('prev').clickAction,prev);
 assert.equal(actions.get('next').clickAction,next);
 const p=registered.get('gex-report-icon-previous').innerHTML;
 const n=registered.get('gex-report-icon-next').innerHTML;
 assert.match(p,/m9 6 6 6-6 6/);
 assert.match(n,/m15 6-6 6 6 6/);
});
test('DevExpress official localization handler receives Persian messages',()=>{
 const {cb}=load();
 let received=null;
 const sender={UpdateLocalization:map=>received=map};
 cb.onCustomizeLocalization(sender,{});
 for(const [en,fa] of Object.entries({'Whole Page':'تمام صفحه','Page Width':'عرض صفحه','Print':'چاپ','Search':'جست‌وجو',
 'Next Page':'صفحه بعد','Previous Page':'صفحه قبل','Export Options':'تنظیمات خروجی'})){
  assert.equal(received[en],fa);
 }
 assert.ok(Object.keys(received).length>=35);
 assert.doesNotThrow(()=>cb.onCustomizeLocalization({},{}));
 assert.doesNotThrow(()=>cb.onCustomizeLocalization(null,{UpdateLocalization:()=>{}}));
 assert.match(viewer,/CustomizeLocalization="reportingCustomization.onCustomizeLocalization"/);
});
test('viewer is single-line, pager vertically centered, permits overflow only along x',()=>{
 assert.match(style,/display: flex !important/);
 assert.match(style,/flex-wrap: nowrap !important/);
 assert.match(style,/white-space: nowrap !important/);
 assert.match(style,/overflow-x: auto !important/);
 assert.match(style,/\.dxrd-preview-pager\.dxrd-toolbar-item/);
 assert.match(style,/display: inline-flex !important/);
 assert.match(style,/height: 64px/);
});
test('native DevExpress print dialog has opening animation but respects reduced motion',()=>{
 assert.match(style,/@keyframes gex-report-print-enter/);
 assert.match(style,/\.dxrd-print-dialog/);
 assert.match(style,/animation: gex-report-print-enter/);
 assert.match(style,/@media \(prefers-reduced-motion: reduce\)/);
 assert.match(style,/animation: none/);
});

test('mobile prioritizes print/export actions without changing native handlers or desktop order', () => {
 function run(width) {
  const templates=new Map();
  const ids={Print:'print',PrintPage:'printPage',ExportTo:'export',PrevPage:'previous',NextPage:'next',ZoomIn:'zoom'};
  const actions=['zoom','previous','printPage','export','next','print'].map(id=>({id,clickAction:()=>id}));
  const original=actions.map(a=>a.id);
  const handlers=new Map(actions.map(a=>[a.id,a.clickAction]));
  const doc={getElementById:id=>templates.get(id),createElement:()=>({}),head:{appendChild:x=>templates.set(x.id,x)}};
  const win={innerWidth:width,DevExpress:{Reporting:{Viewer:{ActionId:ids}}}};
  runInNewContext(script,{window:win,document:doc});
  win.reportingCustomization.onCustomizeMenuActions(null,{Actions:actions,GetById:id=>actions.find(a=>a.id===id)});
  for(const a of actions) assert.equal(a.clickAction,handlers.get(a.id));
  return {after:actions.map(a=>a.id),original,templates};
 }
 const desktop=run(1440);
 assert.deepEqual(desktop.after,desktop.original);
 const mobile=run(390);
 assert.deepEqual(mobile.after.slice(0,3),['printPage','export','print']);
 assert.deepEqual(mobile.after.slice(3),['zoom','previous','next']);
 assert.match(mobile.templates.get('gex-report-icon-previous').innerHTML,/data-gex-icon="previous"/);
});
test('desktop toolbar and print popup are responsive', () => {
 assert.match(style,/justify-content: safe center !important/);
 assert.match(style,/width: 24px/);
 assert.match(style,/direction: ltr !important/);
 assert.match(style,/scaleX\(-1\)/);
 assert.match(style,/width: calc\(100vw - 24px\) !important/);
 assert.match(style,/gex-report-print-centered-enter/);
 assert.match(style,/translate: 0 14px/);
});
