import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
const root = new URL('../../src/App/Client/GoldEx.Client/Pages/Reporting/', import.meta.url);
const read = path => readFileSync(new URL(path,root),'utf8');

test('report filter uses one shared accessible form, action, and RTL date range',()=>{
 const s=read('Components/ReportFiltersBase.razor');
 assert.match(s,/<EditForm Model="Model" OnValidSubmit="Submit"/);
 assert.match(s,/<MudDateRangePicker/);
 assert.match(s,/Adornment="Adornment.Start"/);
 assert.match(s,/gex-filter-date-range/);
 assert.match(s,/@ChildContent/);
 assert.match(s,/OnSubmit.InvokeAsync\(Model\)/);
 assert.doesNotMatch(s,/Elevation="24"|<MudSpacer\s*\/>/);
});
test('report routes use only shared AppBar breadcrumb',()=>{
 const pages=['Index.razor',...readdirSync(new URL('Pages/Reports/',root)).filter(x=>x.endsWith('.razor')).map(x=>'Pages/Reports/'+x)];
 assert.ok(pages.length >= 15);
 for(const page of pages){
  const s=read(page);
  assert.doesNotMatch(s,/<MudBreadcrumbs/,page);
  assert.match(s,/gex-report-page/,page);
 }
});
test('customer balance keeps full summary while virtualizing desktop rows and mobile cards',()=>{
 const view=read('Components/Views/CustomerBalanceReportView.razor');
 const logic=read('Components/Views/CustomerBalanceReportView.razor.cs');
 assert.doesNotMatch(view,/<MudTable/);
 assert.match(view,/balance-report-desktop/);
 assert.match(view,/balance-report-mobile/);
 assert.equal((view.match(/<Virtualize/g)||[]).length,2);
 assert.doesNotMatch(view,/balance-report-pagination|_pageSize|_pageIndex/);
 assert.match(view,/OnViewSource\(row\)/);
 assert.match(view,/<CustomerBalanceSummary Summary="@_summary"/);
 assert.match(logic,/Items.OrderBy/);
 assert.match(logic,/sorted.ToList/);
 assert.match(logic,/GroupBy\(x => x.PriceUnitTitle\)/);
 assert.match(logic,/g.Sum\(x => x.PayableAmount\)/);
 assert.match(logic,/g.Sum\(x => x.ReceivableAmount\)/);
 assert.doesNotMatch(logic,/_pageIndex|_pageSize|PageCount/);
});
test('remaining legacy report tables retain services and have neutral shadow',()=>{
 const names=['SellInvoiceReportView','PurchaseInvoiceReportView','CustomerTransactionsReportView',
  'ProductInventoryReportView','InvoicePaymentReportView','PaymentReportView',
  'CurrencyInventoryReportView','CoinInventoryReportView','InventoryKardexReportView'];
 for(const name of names){
  const s=read('Components/Views/'+name+'.razor');
  assert.match(s,/<MudTable/,name);
  assert.doesNotMatch(s,/Elevation="24"/,name);
 }
});