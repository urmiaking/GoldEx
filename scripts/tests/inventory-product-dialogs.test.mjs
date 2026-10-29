import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const read = path => readFileSync(new URL('../../src/App/Client/GoldEx.Client/Pages/'+path,import.meta.url),'utf8');
const inventory = read('InventoryStocks/Components/InventoryDirectory.razor.cs');
const productRazor = read('Products/Components/Editor.razor');
const productCode = read('Products/Components/Editor.razor.cs');
const productStyle = read('Products/Components/Editor.razor.css');
const vitrineRazor = read('Products/Components/VitrineQuickEditDialog.razor');
const vitrineCode = read('Products/Components/VitrineQuickEditDialog.razor.cs');
const vitrineStyle = read('Products/Components/VitrineQuickEditDialog.razor.css');

test('inventory uses the same responsive dialog infrastructure as new customer',()=>{
 assert.match(inventory,/ResponsiveDialogs\.Show<VitrineQuickEditDialog>\(/);
 assert.match(inventory,/ResponsiveDialogs\.Show<Editor>\(/);
 assert.match(inventory,/wide: true/);
 assert.doesNotMatch(inventory,/DialogService\.ShowAsync<(?:VitrineQuickEditDialog|Editor)>/);
 for(const razor of [productRazor,vitrineRazor]){
  assert.match(razor,/<EditorDialogFrame>/);
  assert.match(razor,/<DialogContent>/);
  assert.match(razor,/<DialogActions>/);
  assert.doesNotMatch(razor,/<MudDialog>/);
 }
});

test('product editor preserves current model, validation and API while adapting session completion',()=>{
 assert.match(productRazor,/<MudForm Model="Model"/);
 assert.match(productCode,/_form\.ValidateAsync\(\)/);
 assert.match(productCode,/Model\.Id\.Value, request/);
 assert.match(productCode,/Session\?\.SetBusy\(true\)/);
 assert.match(productCode,/Session\?\.SetBusy\(false\)/);
 assert.match(productCode,/Session\.Close\(true\)/);
 assert.match(productCode,/Session\.Cancel\(\)/);
 assert.match(productCode,/MudDialog\.Close\(DialogResult\.Ok\(true\)\)/);
 assert.match(productCode,/MudDialog\.Cancel\(\)/);
 assert.match(productRazor,/gex-product-editor-intro/);
 assert.match(productStyle,/\.gex-product-editor-intro/);
});

test('vitrine dialog keeps attributes, toggles, description, gallery, URL and previous backend',()=>{
 for(const field of ['Model.ShowInVitrine','Model.IsFeatured','Model.VitrineDescription','_categoryAttributes','Model.Images','PublicVitrineUrl','CopyVitrineUrlAsync','SetMainImage','RemoveImage','UploadImageAsync']) {
  assert.ok(vitrineRazor.includes(field),field);
 }
 assert.match(vitrineCode,/s\.GetCategoryAttributesAsync/);
 assert.match(vitrineCode,/UpdateProductVitrineAsync\(/);
 assert.match(vitrineCode,/missingRequired/);
 assert.match(vitrineCode,/Model\.AttributeValues/);
 assert.match(vitrineCode,/UploadProductImage\(/);
 assert.match(vitrineCode,/Complete\(Model\)/);
 assert.match(vitrineCode,/Session\.Close\(model\)/);
 assert.match(vitrineCode,/MudDialog\.Close\(DialogResult\.Ok\(model\)\)/);
});

test('vitrine upload/save is guarded against duplicate requests and accidental close',()=>{
 assert.match(vitrineCode,/if \(file == null \|\| _isUploading \|\| _isSaving\) return/);
 assert.match(vitrineCode,/if \(_isSaving \|\| _isUploading\) return/);
 assert.match(vitrineCode,/Session\?\.SetBusy\(true\)/);
 assert.match(vitrineCode,/Session\?\.SetBusy\(false\)/);
 assert.match(vitrineCode,/if \(_isSaving \|\| _isUploading\) return;\s+if \(Session is not null\) Session\.Cancel\(\)/);
 assert.match(vitrineRazor,/Disabled="@\(_isSaving \|\| _isUploading\)"/);
});

test('redesigned vitrine has readable sections, responsive gallery and no inline black backgrounds',()=>{
 for(const section of ['gex-vitrine-overview','gex-vitrine-section','gex-vitrine-switch-grid','gex-vitrine-gallery','gex-vitrine-photo','gex-vitrine-link-actions']){
  assert.ok(vitrineRazor.includes(section),section);
  assert.ok(vitrineStyle.includes('.'+section),section);
 }
 assert.doesNotMatch(vitrineRazor,/background:\s*#000/);
 assert.match(vitrineStyle,/var\(--mud-palette-surface\)/);
 assert.match(vitrineStyle,/var\(--mud-palette-lines-default\)/);
 assert.match(vitrineStyle,/@media\(max-width:640px\)/);
 assert.match(vitrineStyle,/@media\(prefers-reduced-motion:reduce\)/);
});
