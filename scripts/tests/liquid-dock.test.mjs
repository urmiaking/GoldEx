import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import vm from 'node:vm';
import {test} from 'node:test';

const read=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const js=read('src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');
const appbar=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor');
const drawer=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor');

function fixture(count=12){
 const events=obj=>{
  obj.listeners=new Map();
  obj.addEventListener=(k,fn)=>obj.listeners.set(k,fn);
  obj.removeEventListener=k=>obj.listeners.delete(k);
  obj.fire=(k,arg={})=>obj.listeners.get(k)?.(arg);
  return obj;
 };
 const classes=()=>{
  const values=new Set();
  return {add:k=>values.add(k),toggle:(k,on)=>on?values.add(k):values.delete(k),contains:k=>values.has(k)};
 };
 const links=Array.from({length:count},(_,i)=>{
  const attrs=new Map();
  const link=events({
   textContent:'صفحه '+i,title:'',classList:classes(),
   getAttribute:k=>attrs.get(k),
   setAttribute:(k,v)=>attrs.set(k,v),
   removeAttribute:k=>attrs.delete(k),
   querySelector:()=>({textContent:'صفحه '+i}),
   click(){this.clicked=(this.clicked||0)+1;},
   cloneNode(){
    const proxy=events({classList:classes(),removeAttribute(){},click(){this.fire('click',{preventDefault(){}});}});
    return proxy;
   }
  }); return link;
 });
 const nav=events({querySelectorAll:()=>links});
 const viewport={clientHeight:360,querySelector:()=>({getBoundingClientRect:()=>({height:48})})};
 const trigger=events({attrs:new Map(),setAttribute(k,v){this.attrs.set(k,v);},focus(){this.focused=true;}});
 const flyout={inert:false,attrs:new Map(),setAttribute(k,v){this.attrs.set(k,v);}};
 const outlet={children:[],replaceChildren(fragment){this.children=fragment?.children||[];}};
 const more={classList:classes(),querySelector:s=>({
  '.gex-dock-more-trigger':trigger,'.gex-dock-overflow-panel':flyout,'.gex-dock-overflow-links':outlet
 })[s],contains:node=>node===trigger||node===flyout};
 const dock=events({querySelector:s=>({
  '.gex-navigation-menu':nav,'.gex-menu-scroll':viewport,'.gex-dock-more':more
 })[s]});
 const media=events({matches:true});
 const doc=events({createDocumentFragment:()=>({children:[],appendChild(n){this.children.push(n);}})});
 const win=events({});
 const pending=[];
 class Observer{constructor(fn){this.fn=fn;}observe(){}disconnect(){this.disconnected=true;}}
 const context={
  matchMedia:()=>media,document:doc,window:win,
  ResizeObserver:Observer,MutationObserver:Observer,
  requestAnimationFrame:fn=>(pending.push(fn),pending.length),
  cancelAnimationFrame(){}
 };
 vm.runInNewContext(js.replace('export function bind','function bind')+'\nthis.bind=bind;',context);
 const binding=context.bind(dock);
 const flush=()=>{while(pending.length)pending.shift()();};
 flush();
 return {links,viewport,trigger,flyout,outlet,more,dock,media,doc,win,binding,flush};
}
test('viewport-aware native links use three-dot overflow and preserve original navigation',()=>{
 const x=fixture();
 assert.equal(x.links.filter(link=>link.getAttribute('data-dock-overflow')==='false').length,5);
 assert.equal(x.outlet.children.length,7);
 assert.equal(x.more.classList.contains('has-many'),true);
 x.trigger.fire('click');
 assert.equal(x.trigger.attrs.get('aria-expanded'),'true');
 assert.equal(x.flyout.inert,false);
 x.outlet.children[0].fire('click',{preventDefault(){}});
 assert.equal(x.links[5].clicked,1);
 assert.equal(x.trigger.attrs.get('aria-expanded'),'false');
 x.binding.dispose();
});
test('resizing reallocates items, mobile restores links, Escape and disposal release listeners',()=>{
 const x=fixture(11);
 x.viewport.clientHeight=700;
 x.binding.recalculate();
 assert.equal(x.outlet.children.length,0);
 x.viewport.clientHeight=310;
 x.binding.recalculate();
 x.trigger.fire('click');
 let prevented=false;
 x.dock.fire('keydown',{key:'Escape',preventDefault(){prevented=true;}});
 assert.equal(prevented,true);
 assert.equal(x.trigger.focused,true);
 x.media.matches=false;
 x.media.fire('change');
 x.flush();
 assert.equal(x.links.every(l=>l.getAttribute('data-dock-overflow')===undefined),true);
 assert.equal(x.flyout.inert,false);
 x.binding.dispose();
 assert.equal(x.doc.listeners.size,0);
 assert.equal(x.win.listeners.size,0);
});
test('desktop-only liquid glass has mobile, accessibility and account-store fallbacks',()=>{
 assert.match(appbar,/gex-desktop-brand.*ShellBrand Compact="true"/);
 assert.match(appbar,/gex-mobile-brand.*ShellBrand Compact="true"/);
 assert.match(drawer,/class="gex-sidebar gex-liquid-dock/);
 assert.match(drawer,/<NavMenu OnNavigate="HandleNavigate"/);
 assert.match(drawer,/<ShellIdentity/);
 assert.match(drawer,/<LicenseStatusBanner/);
 assert.match(drawer,/liquid-dock\.js/);
 assert.match(css,/backdrop-filter: blur\(27px\) saturate\(185%\)/);
 assert.match(css,/data-dock-overflow="true"/);
 assert.match(css,/@media \(max-width: 959\.98px\)[\s\S]*?\.gex-dock-overflow-panel \{ display: contents; \}/);
 assert.match(css,/@media \(prefers-reduced-motion: reduce\)[\s\S]*?\.gex-liquid-dock \*/);
});
