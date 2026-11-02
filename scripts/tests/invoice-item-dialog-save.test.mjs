import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';
const root=resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const read=(rel)=>readFileSync(resolve(root,rel),'utf8');
const base='src/App/Client/GoldEx.Client/Pages/Invoices/Components/';
const parent=read(base+'EditorForm.razor.cs');
test('coin and currency decimal inputs preserve fractional typing',()=>{
 const coin=read(base+'CoinItemEditor.razor'),currency=read(base+'CurrencyItemEditor.razor');
 assert.doesNotMatch(coin,/DebounceInterval="1"/);
 assert.doesNotMatch(currency,/DebounceInterval="1"/);
 for(const field of ['Model.UnitPrice','Model.CoinInstance.Weight','Model.CoinInstance.Fineness']){
  const markup=[...coin.matchAll(/<MudNumericField\b[\s\S]*?\/>/g)].map(x=>x[0]).find(x=>x.includes(field));
  assert.ok(markup,'Missing '+field);
  assert.match(markup,/Immediate="false"/);
 }
 assert.match(coin,/Immediate="true"/);
});
test('product and coin wait for barcode and list insertion before closing',()=>{
 for(const [component,model,callback,add] of [
  ['ProductItemEditor','ProductItemVm','PrepareAndAddProductItemAsync','_model.AddProductItem(productItem)'],
  ['CoinItemEditor','CoinItemVm','PrepareAndAddCoinItemAsync','_model.AddCoinItem(coinItem)']
 ]){
  const editor=read(base+component+'.razor.cs'),razor=read(base+component+'.razor');
  assert.ok(editor.includes('Func<'+model+', Task<bool>>? PrepareAndAddAsync'));
  assert.match(editor,/if \(_isProcessing\) return;/);
  assert.match(editor,/Session\?\.SetBusy\(true\)/);
  assert.match(editor,/await PrepareAndAddAsync\(Model\)/);
  assert.ok(editor.indexOf('await PrepareAndAddAsync(Model)')<editor.indexOf('Session.Close(Model)'));
  assert.match(editor,/Session\?\.SetBusy\(false\)/);
  assert.match(razor,/Disabled="@_isProcessing"/);
  assert.match(razor,/MudProgressCircular/);
  assert.match(razor,/در حال ذخیره/);
  assert.ok(parent.includes('nameof('+component+'.PrepareAndAddAsync)] = (Func<'+model+', Task<bool>>)'+callback));
  const a=parent.indexOf('private async Task<bool> '+callback),b=parent.indexOf('private async Task OnEdit',a);
  assert.ok(a>0&&b>a);
  const body=parent.slice(a,b);
  assert.ok(body.includes('await AssignReservedBarcodeIfNeeded'));
  assert.ok(body.includes(add));
  assert.ok(body.indexOf('await AssignReservedBarcodeIfNeeded')<body.indexOf(add));
 }
});
test('overlay clicks never dismiss native or Mud dialogs',()=>{
 const native=read('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor');
 const layout=read('src/App/Client/GoldEx.Client.Components/Layouts/BaseLayout.razor');
 assert.match(native,/<div class="gex-modal-backdrop" aria-hidden="true"><\/div>/);
 assert.doesNotMatch(native,/gex-modal-backdrop"[^>]*@onclick/);
 assert.match(native,/gex-modal-close"[^>]*@onclick="Close"/);
 assert.match(layout,/MudDialogProvider BackdropClick="false" CloseOnEscapeKey="false"/);
});