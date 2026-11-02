import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
const root=new URL('../../src/App/Client/GoldEx.Client/Pages/',import.meta.url);
const read=p=>readFileSync(new URL(p,root),'utf8');

test('melting wizard has RTL back chevron and does not reuse reversed arrow',()=>{
 const view=read('InventoryStocks/Components/MeltingBatchForm.razor');
 assert.match(view,/StartIcon="@Icons.Material.Outlined.ArrowForward" OnClick="BackToList"/);
});
test('shared selector passes actual query value, not the SearchQuery literal',()=>{
 const parent=read('InventoryStocks/Components/InventoryStockList.razor');
 const child=read('InventoryStocks/Components/InventoryStockSelection.razor');
 assert.match(parent,/SearchQuery="@SearchQuery"/);
 assert.doesNotMatch(parent,/SearchQuery="SearchQuery"/);
 assert.match(child,/Value="@_search" ValueChanged="OnSearch"/);
 assert.doesNotMatch(child,/stock-selector-advanced/);
 assert.match(child,/OnClick="OpenFilters"/);
});
test('selector keeps a real responsive filter dialog and used gold is locked to its single type',()=>{
 const dialog=read('InventoryStocks/Components/InventoryStockSelectionFiltersDialog.razor');
 const logic=read('InventoryStocks/Components/InventoryStockSelection.razor.cs');
 const vm=read('InventoryStocks/Components/InventoryStockSelectionFiltersDialog.razor.cs');
 assert.match(dialog,/<EditorDialogFrame>/);
 assert.match(dialog,/@if \(SelectableTypes.Length > 1\)/);
 assert.match(dialog,/<MudDateRangePicker/);
 assert.match(dialog,/gex-filter-date-range/);
 assert.match(vm,/FilterResult\(/);
 assert.match(logic,/ResponsiveDialogs.Show<InventoryStockSelectionFiltersDialog>/);
 assert.match(logic,/if \(selectionChanged\) await ClearSelection\(\)/);
});
test('reports have exactly one apply button, not two non-working responsive copies',()=>{
 const view=read('Reporting/Components/ReportFiltersBase.razor');
 assert.equal(view.match(/ButtonType="ButtonType.Submit"/g)?.length,1);
 assert.match(view,/Class="report-filter-action-item"/);
 assert.match(view,/@ChildContent/);
 assert.match(view,/OnSubmit.InvokeAsync\(Model\)/);
});
test('profit analysis has mobile cards, one print trigger and full data summary',()=>{
 const view=read('Reporting/Components/Views/ProfitAnalysisReportView.razor');
 const logic=read('Reporting/Components/Views/ProfitAnalysisReportView.razor.cs');
 assert.doesNotMatch(view,/<MudTable/);
 assert.match(view,/profit-analysis-mobile/);
 assert.match(view,/profit-analysis-desktop/);
 assert.equal(view.match(/OnPrintReport.InvokeAsync/g)?.length,1);
 assert.match(view,/<ProfitAnalysisSummary Report="Report"/);
 assert.match(logic,/Report\?\.Items.ToList/);
 assert.equal((view.match(/<Virtualize/g)||[]).length,2);
 assert.doesNotMatch(view,/profit-analysis-pagination/);
});
test('manual inventory entry opens product, coin, currency and Excel native modal and confirms explicitly',()=>{
 const base='InventoryEntry/Components/';
 for(const name of ['ProductList','CoinList','CurrencyList']){
   const code=read(base+name+'.razor.cs');
   assert.match(code,/ResponsiveDialogs.Show/);
   assert.doesNotMatch(code,/DialogService.ShowAsync/);
   assert.doesNotMatch(code,/ShowMessageBoxAsync/);
   assert.match(code,/Data: true/);
 }
 const excel=read(base+'ExcelUploadDialog.razor');
 assert.match(excel,/<EditorDialogFrame>/);
 assert.doesNotMatch(excel,/<MudDialog|<style>/);
 assert.match(read(base+'ExcelUploadDialog.razor.cs'),/Session.Close\(result\)/);
 assert.match(read(base+'InventoryEntryContainer.razor.cs'),/ResponsiveDialogs.Show<InventoryEntryConfirmDialog>/);
 assert.match(read(base+'InventoryEntryList.razor.cs'),/ResponsiveDialogs.Show<InventoryEntryConfirmDialog>/);
});