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
  obj.addEventListener=(k,fn)=>obj.listeners.set(k,[...(obj.listeners.get(k)||[]),fn]);
  obj.removeEventListener=(k,fn)=>{
    const remaining=(obj.listeners.get(k)||[]).filter(f=>f!==fn);
    if(remaining.length) obj.listeners.set(k,remaining); else obj.listeners.delete(k);
  };
  obj.fire=(k,arg={})=>(obj.listeners.get(k)||[]).forEach(fn=>fn(arg));
  return obj;
 };
 const classes=()=>{
  const values=new Set();
  return {add:k=>values.add(k),remove:k=>values.delete(k),toggle:(k,on)=>on?values.add(k):values.delete(k),contains:k=>values.has(k)};
 };
 const links=Array.from({length:count},(_,i)=>{
  const attrs=new Map();
  const link=events({
   textContent:'صفحه '+i,title:'',classList:classes(),
   getBoundingClientRect:()=>({top:120+i*52}),
   closest(selector){return selector==='.gex-sidebar-identity'?null:this;},
   matches:()=>false,
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
 const trigger=events({attrs:new Map(),getBoundingClientRect:()=>({top:600}),setAttribute(k,v){this.attrs.set(k,v);},focus(){this.focused=true;}});
 const flyout={inert:false,attrs:new Map(),setAttribute(k,v){this.attrs.set(k,v);}};
 const outlet={children:[],replaceChildren(fragment){this.children=fragment?.children||[];}};
 const more={classList:classes(),querySelector:s=>({
  '.gex-dock-more-trigger':trigger,'.gex-dock-overflow-panel':flyout,'.gex-dock-overflow-links':outlet
 })[s],contains:node=>node===trigger||node===flyout};
 const drop=events({});
 const glass={style:{clipPath:""}};
 const props=new Map();
 const dock=events({classList:classes(),style:{setProperty:(k,v)=>props.set(k,v),getPropertyValue:k=>props.get(k)},
  getBoundingClientRect:()=>({top:60,height:740}),contains:()=>true,
  querySelector:s=>({
    '.gex-navigation-menu':nav,'.gex-menu-scroll':viewport,'.gex-dock-more':more,
    '.gex-dock-active-drop':drop,'.gex-dock-glass':glass
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
 links[0].classList.add('active');
 const binding=context.bind(dock);
 const flush=()=>{while(pending.length)pending.shift()();};
 flush();
 return {links,nav,viewport,trigger,flyout,outlet,more,dock,drop,glass,media,doc,win,binding,flush};
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

test('one connected glass lobe smoothly follows hover across different native links',()=>{
 const x=fixture();
 x.dock.fire('pointerover',{target:x.links[1]});
 assert.equal(x.dock.classList.contains('gex-dock-has-hover'),true);
 assert.equal(x.dock.style.getPropertyValue('--gex-lobe-top'),'112px');
 assert.equal(x.dock.style.getPropertyValue('--gex-lobe-width'),'220px');
 x.dock.fire('pointerover',{target:x.links[2]});
 assert.equal(x.dock.style.getPropertyValue('--gex-lobe-top'),'164px');
 x.dock.fire('pointerout',{target:x.links[2]});
 assert.equal(x.dock.classList.contains('gex-dock-has-hover'),false);
 x.dock.fire('focusin',{target:x.links[3]});
 assert.equal(x.dock.classList.contains('gex-dock-has-hover'),true);
 x.binding.dispose();
 assert.equal(x.drop.listeners.size,0);
 assert.equal(x.dock.listeners.size,0);
});

test('route selection droplet glides from home to calculator and settles on NavLink active change',()=>{
 const x=fixture();
 assert.equal(x.dock.style.getPropertyValue('--gex-drop-top'),'62px');
 x.nav.fire('click',{target:x.links[2]});
 assert.equal(x.dock.style.getPropertyValue('--gex-drop-top'),'166px');
 assert.equal(x.dock.classList.contains('gex-dock-drop-traveling'),true);
 x.links[0].classList.remove('active');
 x.links[2].classList.add('active');
 x.binding.recalculate();
 assert.equal(x.dock.style.getPropertyValue('--gex-drop-top'),'166px');
 x.drop.fire('transitionend',{target:x.drop,propertyName:'top'});
 assert.equal(x.dock.classList.contains('gex-dock-drop-traveling'),false);
 x.binding.dispose();
});

test('account logout text and current store logo are accessible; glass lobe is a dock child',()=>{
 const identity=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/ShellIdentity.razor');
 const stores=read('src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor');
 assert.match(identity,/<span class="gex-signout-label">خروج از حساب<\/span>/);
 assert.match(css,/\.gex-liquid-dock \.gex-signout-label \{ display: inline;/);
 assert.match(css,/\.gex-liquid-dock \.gex-sidebar-identity:is\(:hover,:focus-within\) \.gex-signout/);
 assert.match(css,/\.gex-liquid-dock \.gex-signout \{[\s\S]*?inset-inline-start: 100%;/);
 assert.match(stores,/src="@_currentStore.LogoUrl"/);
 assert.match(stores,/disabled="@_switchingStore"/);
 assert.match(stores,/if \(_switchingStore \|\| _stores.Count <= 1\) return;/);
 assert.match(css,/\.gex-liquid-dock \.gex-store-picker-logo img \{[\s\S]*?object-fit: contain;/);
 assert.match(css,/\.gex-liquid-dock \.gex-store-picker-chevron \{[\s\S]*?margin-inline-end: 11px;/);
 assert.match(drawer,/class="gex-dock-glass-frame" aria-hidden="true"/);
 assert.match(drawer,/class="gex-dock-active-drop" aria-hidden="true"/);
 assert.match(css,/\.gex-liquid-dock \.gex-dock-glass \{/);
 assert.match(css,/\.gex-liquid-dock\.gex-dock-drop-traveling \.gex-dock-active-drop/);
 assert.match(css,/\.gex-liquid-dock \.gex-navigation-menu > a:is\(:hover,:focus-visible\) \{[\s\S]*?box-shadow: none;/);
});
test('overflow active route uses More droplet while new-tab external links preserve selection',()=>{
 const x=fixture();
 const previous=x.dock.style.getPropertyValue('--gex-drop-top');
 x.links[6].setAttribute('target','_blank');
 x.outlet.children[1].fire('click',{preventDefault(){}});
 assert.equal(x.links[6].clicked,1);
 assert.equal(x.dock.style.getPropertyValue('--gex-drop-top'),previous);
 x.links[0].classList.remove('active');
 x.links[10].classList.add('active');
 x.binding.recalculate();
 assert.equal(x.dock.style.getPropertyValue('--gex-drop-top'),'542px');
 x.binding.dispose();
});
test('desktop hover reshapes the actual one-piece dock glass and account covers bottom edge',()=>{
 const x=fixture();
 const base=x.glass.style.clipPath;
 assert.match(base,/^path\("M 239 0 /);
 assert.match(base,/H 239 Q 208 740 208 709/);
 x.dock.fire('pointerover',{target:x.links[2]});
 const hover=x.glass.style.clipPath;
 assert.notEqual(hover,base);
 assert.match(hover,/L 36 [0-9]/);
 assert.match(css,/\.gex-dock-glass \{[\s\S]*?clip-path: inset/);
 assert.match(css,/transition: clip-path/);
 assert.match(css,/\.goldex-shell \.gex-liquid-dock \{[\s\S]*?background: transparent/);
 assert.doesNotMatch(drawer,/gex-dock-liquid-lobe/);
 const avatar={getBoundingClientRect:()=>({top:715}),matches:()=>false,closest(selector){return selector==='.gex-sidebar-identity'?{className:'gex-sidebar-identity'}:this;}};
 x.dock.fire('pointerover',{target:avatar});
 assert.equal(x.dock.classList.contains('gex-dock-is-account'),true);
 assert.match(x.glass.style.clipPath,/H 49 Q 18 740 18 709 V 709 C 18 709 18 709 18 709 L 18 709/);
 assert.notEqual(x.glass.style.clipPath,hover);
 assert.match(css,/\.gex-liquid-dock\.gex-dock-is-account \.gex-dock-glass/);
 assert.match(css,/\.gex-liquid-dock \.gex-signout,\s*\.gex-liquid-dock \.gex-signout:hover \{[\s\S]*?background: transparent !important/);
 x.dock.fire('pointerout',{target:avatar});
 assert.equal(x.dock.classList.contains('gex-dock-is-account'),false);
 assert.match(x.glass.style.clipPath,/H 239 Q 208 740 208 709/);
 x.binding.dispose();
});