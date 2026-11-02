import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/Finances/CustomerTransfers/', import.meta.url);
const read = path => readFileSync(new URL(path,root),'utf8');

test('main transfers view uses a responsive semantic table and dedicated mobile cards',()=>{
 const page=read('List.razor'), view=read('Components/CustomerTransferList.razor');
 assert.match(page, /<CustomerTransferList/);
 assert.doesNotMatch(view, /<MudTable/);
 assert.match(view, /<table class="transfer-table"/);
 assert.match(view, /class="transfer-mobile"/);
 assert.match(view, /class="transfer-card"/);
 for(const text of ['SourceCustomerName','DestinationCustomerName','PriceUnitTitle','SourceInvoiceNumber','DestinationInvoiceNumber','Description'])
   assert.ok(view.includes(text),text);
 assert.match(view,/transfer-loading/);
 assert.match(view,/transfer-empty/);
 assert.match(view,/transfer-pagination/);
});

test('paging, filtering, searches and sorting are applied to server request before presentation',()=>{
 const code=read('Components/CustomerTransferList.razor.cs');
 assert.match(code,/new RequestFilter\(_pageIndex \* _pageSize, _pageSize/);
 assert.match(code,/SourceCustomerId = CustomerId/);
 assert.match(code,/PriceUnitId = _priceUnitId/);
 assert.match(code,/GetListAsync\(filter, transferFilter, ct\)/);
 assert.match(code,/cancelPrevious: true/);
 assert.match(code,/int _pageIndex, _pageSize = 10/);
 assert.match(code,/RefreshAsync\(\)/);
 assert.match(code,/OnSearch\(string\? query\)/);
 assert.match(code,/GetUriWithQueryParameter\("q"/);
 const dialog=read('Components/CustomerTransferFiltersDialog.razor');
 assert.match(dialog,/MudDateRangePicker/);
 assert.match(dialog,/gex-filter-date-range/);
 assert.match(dialog,/همه واحدها/);
 assert.match(dialog,/gex-filter-mobile-sort/);
});

test('edit, delete and nested new-customer dialogs use the responsive dialog stack',()=>{
 const code=read('Components/CustomerTransferList.razor.cs');
 const editor=read('Components/CustomerTransferEditor.razor');
 const editorCode=read('Components/CustomerTransferEditor.razor.cs');
 const deleteDialog=read('Components/CustomerTransferDeleteDialog.razor');
 for(const type of ['CustomerTransferFiltersDialog','CustomerTransferDeleteDialog','CustomerTransferEditor'])
   assert.ok(code.includes('ResponsiveDialogs.Show<'+type+'>'),type);
 assert.match(editor,/<EditorDialogFrame>/);
 assert.doesNotMatch(editor,/<MudDialog>/);
 assert.match(editorCode,/ResponsiveDialogs.Show<Editor>/);
 assert.match(editorCode,/ReturnModel/);
 assert.match(editorCode,/Session.Close\(true\)/);
 assert.match(deleteDialog,/<EditorDialogFrame>/);
 assert.match(deleteDialog,/حذف حواله/);
 assert.match(editorCode,/SourceProjectedBalance/);
 assert.match(editorCode,/DestProjectedBalance/);
 assert.match(editorCode,/CreateCustomerTransferVoucherRequest/);
 assert.match(editorCode,/UpdateCustomerTransferVoucherRequest/);
});

test('stylesheet provides responsive breakpoints and reduced motion',()=>{
 const css=read('Components/CustomerTransferList.razor.css');
 assert.match(css,/@media\(max-width:959\.98px\)/);
 assert.match(css,/@media\(prefers-reduced-motion:reduce\)/);
 assert.match(css,/transfer-filters \{ position:sticky;/);
 assert.match(css,/transfer-mobile \{ display:none;/);
 assert.match(css,/transfer-desktop \{ display:none;/);
});
