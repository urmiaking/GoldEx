import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/Reporting/Components/', import.meta.url);
const read = p => readFileSync(new URL(p, root), 'utf8');

test('filters align actions with fields, without detached footer or duplicate submit', () => {
 const view=read('ReportFiltersBase.razor');
 const css=read('ReportFiltersBase.razor.css');
 assert.match(view,/report-filter-body/);
 assert.match(view,/Class="report-filter-action-item"/);
 assert.equal((view.match(/ButtonType="ButtonType.Submit"/g)||[]).length,1);
 assert.doesNotMatch(view,/report-filter-actions|report-filter-mobile-submit/);
 assert.match(css,/\.report-filter-body ::deep \.report-filter-fields\.mud-grid/);
 assert.match(css,/align-items:center/);
 assert.doesNotMatch(css,/report-filter-actions/);
});
test('customer balance and profit analysis virtualize both presentations without new pagination',()=>{
 for(const name of ['CustomerBalanceReportView','ProfitAnalysisReportView']) {
  const view=read('Views/'+name+'.razor');
  const logic=read('Views/'+name+'.razor.cs');
  const css=read('Views/'+name+'.razor.css');
  assert.equal((view.match(/<Virtualize /g)||[]).length,2,name);
  assert.match(view,/SpacerElement="tr"/,name);
  assert.doesNotMatch(view,/\b_pageSize\b|\b_pageIndex\b|\bPageCount\b/,name);
  assert.doesNotMatch(logic,/\b_pageSize\b|\b_pageIndex\b|\bPageCount\b/,name);
  assert.doesNotMatch(css,/-pagination/,name);
 }
 assert.match(read('Views/CustomerBalanceReportView.razor'),/<CustomerBalanceSummary Summary="@_summary"/);
 assert.match(read('Views/ProfitAnalysisReportView.razor'),/<ProfitAnalysisSummary Report="Report"/);
});
test('all reporting views show skeleton rows or mobile cards rather than progress bars',()=>{
 const names=readdirSync(new URL('Views/',root)).filter(x=>x.endsWith('ReportView.razor'));
 assert.ok(names.length>=14);
 for(const name of names) {
   const view=read('Views/'+name);
   assert.doesNotMatch(view,/<MudProgressLinear|<MudProgressCircular/,name);
   assert.match(view,/<ReportTableSkeleton\s*\/>/,name);
 }
 const skeleton=read('Views/ReportTableSkeleton.razor');
 assert.match(skeleton,/report-table-skeleton-desktop/);
 assert.match(skeleton,/report-table-skeleton-mobile/);
 assert.match(skeleton,/<MudSkeleton/);
 assert.match(skeleton,/Animation.Wave/);
});
