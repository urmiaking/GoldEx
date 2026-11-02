import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/InventoryStocks/', import.meta.url);
const read = (path) => readFileSync(new URL(path, root), 'utf8');

test('melting batches directory uses compact mobile cards and native desktop table', () => {
  const view = read('Components/MeltingBatchesList.razor');
  const css = read('Components/MeltingBatchesList.razor.css');
  assert.doesNotMatch(view, /<MudTable/);
  assert.match(view, /class="melting-table"/);
  assert.match(view, /class="melting-mobile"/);
  assert.match(view, /class="melting-card"/);
  assert.match(view, /MeltingBatchDetails Batch="item"/);
  assert.match(view, /melting-skeleton/);
  assert.match(view, /melting-empty/);
  assert.match(view, /melting-pagination/);
  assert.match(css, /max-width:959\.98px/);
  assert.match(css, /\.melting-toolbar ::deep \.melting-sort/);
});
test('list keeps backend request contract and action routes', () => {
  const code = read('Components/MeltingBatchesList.razor.cs');
  assert.match(code, /new RequestFilter\(_pageIndex \* _pageSize, _pageSize/);
  assert.match(code, /new MeltingBatchFilter\(_status, _filterDateRange.Start, _filterDateRange.End\)/);
  assert.match(code, /SendRequestAsync<IMeltingBatchService, PagedList<GetMeltingBatchResponse>>/);
  assert.match(code, /cancelPrevious: true/);
  assert.match(code, /NavigateTo\(ClientRoutes.InventoryStocks.MeltingBatches.Create\)/);
  assert.match(code, /Set.FormatRoute\(new \{ id = batchId \}\)/);
  assert.match(code, /InvokeVoidAsync\("printBarcode", labelData\)/);
  assert.match(code, /\b_pageSize = 10\b/);
  assert.match(code, /ResponsiveDialogs.Show<MeltingBatchFiltersDialog>/);
});
test('filter modal supports date/status, sort on mobile and RTL calendar icon', () => {
  const view = read('Components/MeltingBatchFiltersDialog.razor');
  const logic = read('Components/MeltingBatchFiltersDialog.razor.cs');
  assert.match(view, /<EditorDialogFrame>/);
  for(const label of ['همه درخواست‌ها','تاریخ ثبت','وضعیت درخواست','مرتب‌سازی بر اساس','بازنشانی'])
    assert.ok(view.includes(label), label);
  assert.match(view, /gex-filter-date-range/);
  assert.match(view, /Adornment="Adornment.Start"/);
  assert.match(view, /gex-filter-mobile-sort/);
  assert.match(logic, /Session\?\.Close\(new FilterResult/);
});
test('expanded details retain original products, final assaying results, losses, transactions and barcode', () => {
  const view = read('Components/MeltingBatchDetails.razor');
  for (const key of ['Batch.Products','FinalProductDetail','WasteWeight','WastePercentage','Batch.Transactions','OnPrintBarcode'])
    assert.ok(view.includes(key),key);
  assert.doesNotMatch(view, /<MudTable/);
});
test('wizard preserves all three financial business operations and uses native step confirmation', () => {
  const view = read('Components/MeltingBatchForm.razor');
  const code = read('Components/MeltingBatchForm.razor.cs');
  for(const step of ['ProductSelectionStep','LabSubmissionStep','MeltedGoldStep'])
    assert.ok(view.includes(step),step);
  for(const serviceCall of ['CreateAsync(_model.ToMeltingRequest()','SendToLabAsync(_id.Value, _model.ToSendToLabRequest()','CompleteMeltingAsync(_id.Value, _model.ToCompleteMeltingRequest()'])
    assert.ok(code.includes(serviceCall),serviceCall);
  assert.match(code,/ResponsiveDialogs.Show<MeltingBatchConfirmDialog>/);
  assert.match(code,/ResponsiveDialogs.Show<FinancialAccountEditor>/);
  assert.match(read('Components/MeltingBatchConfirmDialog.razor'),/<EditorDialogFrame>/);
  assert.match(read('Components/MeltingBatches/LabSubmissionStep.razor.cs'),/ResponsiveDialogs.Show<Customers.Components.Editor>/);
});
test('routes rely on shared breadcrumbs instead of duplicate old MudBreadcrumbs', () => {
  for(const page of ['MeltingBatches.razor','SetMeltingBatch.razor']){
    const view=read(page);
    assert.doesNotMatch(view,/<MudBreadcrumbs/);
  }
});
