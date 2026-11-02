import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {runInNewContext} from 'node:vm';
const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const code=read('src/App/Client/GoldEx.Client/wwwroot/js/reporting-customization.js');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/devexpress-viewer-fixes.css');
const app=read('src/App/Server/GoldEx.Server/Components/App.razor');

function setup(){
 const ids={ExportTo:'export',Print:'print',FirstPage:'first',LastPage:'last',PrevPage:'prev',NextPage:'next',Pagination:'pager',MultipageToggle:'multi'};
 const actions=new Map(Object.entries(ids).map(([name,id])=>[id,{id,visible:true,clickAction:()=>name+' works'}]));
 const nested=[{imageTemplateName:'original-icon',items:[{format:'pdf'}]}];
 actions.get('export').items=()=>nested;
 const inserted=new Map();
 const doc={getElementById:id=>inserted.get(id),createElement:()=>({}),head:{appendChild:e=>inserted.set(e.id,e)}};
 const win={innerWidth:1440,DevExpress:{Reporting:{Viewer:{ActionId:ids}}}};
 runInNewContext(code,{window:win,document:doc});
 return {cb:win.reportingCustomization.onCustomizeMenuActions,actions,nested,inserted};
}
test('export icon applies to the real dropdown items()[0] and retains native click handler',()=>{
 const {cb,actions,nested,inserted}=setup();
 const old=actions.get('export').clickAction;
 cb(null,{GetById:id=>actions.get(id)});
 assert.equal(nested[0].imageTemplateName,'gex-report-icon-export');
 assert.equal(actions.get('export').clickAction,old);
 assert.equal(actions.get('export').clickAction(),'ExportTo works');
 assert.equal(nested[0].items[0].format,'pdf');
 assert.match(inserted.get('gex-report-icon-export').innerHTML,/svg/);
});
test('pager uses native Previous, Page Selector, Next; hides redundant actions only',()=>{
 const {cb,actions}=setup();
 const old=new Map([...actions].map(([id,a])=>[id,a.clickAction]));
 cb(null,{GetById:id=>actions.get(id)});
 for(const id of ['first','last','multi'])assert.equal(actions.get(id).visible,false);
 for(const id of ['prev','pager','next'])assert.equal(actions.get(id).visible,true);
 assert.equal(actions.get('prev').imageTemplateName,'gex-report-icon-previous');
 assert.equal(actions.get('next').imageTemplateName,'gex-report-icon-next');
 for(const [id,fn] of old)assert.equal(actions.get(id).clickAction,fn);
});
test('flat export action remains supported',()=>{
 const {cb,actions}=setup();actions.get('export').items=undefined;
 cb(null,{GetById:id=>actions.get(id)});
 assert.equal(actions.get('export').imageTemplateName,'gex-report-icon-export');
});
test('right rail has light-mode hover contrast and branded active state; pager compact',()=>{
 assert.match(css,/\.dxrd-right-tabs/);
 assert.match(css,/\.dxd-back-highlighted:not\(\.dxd-state-active\):hover/);
 assert.match(css,/fill: var\(--gex-report-ink\)/);
 assert.match(css,/background: var\(--gex-report-accent\)/);
 assert.match(css,/fill: var\(--gex-report-accent-ink\)/);
 assert.match(css,/\.dxrd-preview-pager-selectbox/);
 assert.match(css,/width: 92px/);
 assert.ok(app.indexOf('css/devexpress-viewer-fixes.css')>app.indexOf('css/devexpress-viewer-responsive.css'));
});
