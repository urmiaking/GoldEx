import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const at=p=>readFileSync(new URL('../../'+p,import.meta.url),'utf8');
const base='src/App/Client/GoldEx.Client/Pages/';
const product=at(base+'Products/Components/Editor.razor.cs');
const productRazor=at(base+'Products/Components/Editor.razor');
const categories=at(base+'Settings/Components/Categories/Editor.razor.cs');
const categoryRazor=at(base+'Settings/Components/Categories/Editor.razor');
const modalCss=at('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialog.razor.css');

test('nested new-category editor opens above product edit using same session stack',()=>{
 assert.match(product,/if \(Session is not null\)/);
 assert.match(product,/ResponsiveDialogs\.Show<Settings\.Components\.Categories\.Editor>/);
 assert.match(product,/await nested\.Result/);
 assert.match(product,/await LoadCategoriesAsync\(\)/);
 assert.match(product,/DialogService\.ShowAsync<Settings\.Components\.Categories\.Editor>/);
 assert.match(categoryRazor,/<EditorDialogFrame>/);
 assert.match(categories,/ResponsiveDialogSession\? Session/);
 assert.match(categories,/Session\.Close\(true\)/);
 assert.match(categories,/Session\.Cancel\(\)/);
 assert.match(categories,/MudDialog\.Close\(DialogResult\.Ok\(true\)\)/);
 assert.match(categories,/Session\?\.SetBusy\(true\)/);
 assert.match(categories,/Session\?\.SetBusy\(false\)/);
 assert.match(modalCss,/\.gex-modal-session > form \{ display:flex/);
});

test('product save and upload prevent accidental closure or simultaneous request',()=>{
 assert.match(product,/if \(_processing \|\| _isUploadingImage\) return/);
 assert.match(product,/if \(file == null \|\| _processing \|\| _isUploadingImage\) return/);
 assert.match(product,/Session\?\.SetBusy\(true\)/);
 assert.match(product,/Session\?\.SetBusy\(false\)/);
 assert.match(productRazor,/Disabled="@\(_processing \|\| _isUploadingImage\)"/);
 assert.match(productRazor,/class="gex-product-edit-gallery"/);
 assert.match(productRazor,/class="gex-product-edit-photo"/);
});
