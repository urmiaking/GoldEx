import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

const base = new URL('../../src/App/', import.meta.url);
const src = path => readFileSync(new URL(path, base), 'utf8');

test('report reads source invoices and authoritative ledger without double counting', () => {
 const service = src('Server/GoldEx.Server.Application/Services/ReportingService.cs');
 for (const value of [
  'GetCustomerComprehensiveAsync',
  'InvoicesByCustomerIdSpecification(new CustomerId(request.CustomerId))',
  '.AsNoTracking()',
  '.AsSplitQuery()',
  '.Include(x => x.ProductItems)',
  '.Include(x => x.InvoicePayments',
  '.Include(x => x.UsedProducts)',
  '.Include(x => x.CoinItems)',
  '.Include(x => x.CurrencyItems)',
  'transactionRepository.GetCustomerTransactionsAsync(ledgerRequest',
  'completeLedger.GroupBy(t => t.PriceUnitTitle)',
  'request.FromDate.Value',
 ]) assert.ok(service.includes(value), value);
 assert.match(service, /new CustomerTransactionRpRequest\(\s*request.CustomerId, null, null, null, request.ToDate\)/);
 assert.doesNotMatch(service, /AccountingTransactionService\.BuildTransactionsForInvoiceAsync/);
});
test('the same report API works through SSR and WASM', () => {
 for (const path of [
  'Shared/GoldEx.Shared/Services/Abstractions/IReportingService.cs',
  'Server/GoldEx.Server.Application/Services/ReportingService.cs',
  'Client/GoldEx.Client.Services/Services/ReportingService.cs',
  'Server/GoldEx.Server/Controllers/ReportingController.cs'
 ]) assert.match(src(path), /GetCustomerComprehensiveAsync/);
 assert.match(src('Shared/GoldEx.Shared/Routings/ClientRoutes.cs'), /CustomerComprehensive = .*customer-comprehensive/);
 assert.match(src('Client/GoldEx.Client/Pages/Customers/Components/CustomerCard.razor'), /OnViewComprehensive/);
});
test('screen and print share one document and GoldEx print infrastructure', () => {
 const screen = src('Client/GoldEx.Client/Pages/Reporting/Pages/Reports/CustomerComprehensive.razor');
 const print = src('Client/GoldEx.Client/Pages/Reporting/Pages/Prints/CustomerComprehensiveReportPrint.razor');
 const document = src('Client/GoldEx.Client/Pages/Reporting/Components/CustomerComprehensiveDocument.razor');
 assert.match(screen, /<CustomerComprehensiveDocument /);
 assert.match(print, /<CustomerComprehensiveDocument /);
 assert.match(print, /<ReportPrintBase /);
 assert.match(print, /report-print.js/);
 for (const source of ['Invoices','Payments','Transactions','Balances']) assert.match(document,new RegExp('Data\\.'+source));
 assert.match(document, /GroupBy\(x => new \{ x.InvoiceType, x.PriceUnitTitle \}\)/);
 const css = src('Server/GoldEx.Server/wwwroot/css/reports/customer-comprehensive.css');
 for(const value of ['@media print','table-header-group','break-inside: avoid']) assert.ok(css.includes(value),value);
});

test('summary has trade totals and balances without meaningless document counts', () => {
 const doc = src('Client/GoldEx.Client/Pages/Reporting/Components/CustomerComprehensiveDocument.razor');
 for (const text of ['gex-customer-overview', 'x.InvoiceType, x.PriceUnitTitle', 'Data.Balances']) {
  assert.ok(doc.includes(text), text);
 }
 for (const text of ['ردیف گردش حساب', 'اسناد پرداخت فاکتور', 'مرجع رسمی مانده‌ها', 'تراکنش‌های معکوس‌شده حذف شده‌اند', 'gex-customer-kpis']) {
  assert.ok(!doc.includes(text), text);
 }
});
test('one grouped invoice table with invoice and item rows, plus one financial table', () => {
 const doc = src('Client/GoldEx.Client/Pages/Reporting/Components/CustomerComprehensiveDocument.razor');
 assert.equal(doc.split('<table ').length - 1, 2);
 for (const text of [
  'gex-customer-invoices-table', '@foreach (var invoice in Data.Invoices)',
  '<tbody class="gex-customer-invoice-group">', 'gex-customer-invoice-group-heading',
  '@foreach (var item in invoice.Items)', 'gex-customer-invoice-adjustments',
  'Data.Payments', 'Data.Transactions', 'paymentsWithLedger.Contains(x.Id)'
 ]) assert.ok(doc.includes(text), text);
});
test('explicit nullable customer name interpolation and selectable portrait/landscape printing', () => {
 const view = src('Client/GoldEx.Client/Pages/Reporting/Pages/Reports/CustomerComprehensive.razor');
 const viewCode = src('Client/GoldEx.Client/Pages/Reporting/Pages/Reports/CustomerComprehensive.razor.cs');
 const print = src('Client/GoldEx.Client/Pages/Reporting/Pages/Prints/CustomerComprehensiveReportPrint.razor');
 const printCode = src('Client/GoldEx.Client/Pages/Reporting/Pages/Prints/CustomerComprehensiveReportPrint.razor.cs');
 const template = src('Client/GoldEx.Client/Pages/Reporting/Components/ReportPrintBase.razor');
 for (const text of ['<p>@(_customer?.FullName)</p>', 'CustomerName="@(_customer?.FullName)"', '<select @bind="_printOrientation"']) {
  assert.ok(view.includes(text), text);
 }
 for (const text of ['CustomerName="@(_customer?.FullName)"', 'Landscape="@LandscapeMode"', 'ShowDetailHeading="false" ShowNotes="false"', 'Data="@_report"']) {
  assert.ok(print.includes(text), text);
 }
 assert.ok(view.includes('<option value="landscape">افقی</option>'));
 assert.ok(view.includes('<option value="portrait">عمودی</option>'));
 assert.ok(viewCode.includes('private string _printOrientation = "landscape";'));
 assert.ok(viewCode.includes('landscape = _printOrientation == "landscape"'));
 assert.ok(!viewCode.includes('landscape = _printLandscape'));
 assert.ok(printCode.includes('SupplyParameterFromQuery(Name = "landscape")'));
 for (const text of [
  '[Parameter] public bool ShowDetailHeading { get; set; } = true;',
  '[Parameter] public bool ShowNotes { get; set; } = true;'
 ]) assert.ok(template.includes(text), text);
});

test('summary-only reports preserve the actual invoice item count independently of detail rows', () => {
 const contract = src('Shared/GoldEx.Shared/DTOs/Reporting/CustomerComprehensiveRpResponse.cs');
 const service = src('Server/GoldEx.Server.Application/Services/ReportingService.cs');
 const report = src('Client/GoldEx.Client/Pages/Reporting/Components/CustomerComprehensiveDocument.razor');
 assert.ok(contract.includes('int ItemCount'), 'item count must be a separate DTO field');
 assert.ok(service.includes('x.ProductItems.Count + x.UsedProducts.Count + x.CoinItems.Count + x.CurrencyItems.Count'),
   'the item count covers all four invoice item kinds, regardless of summary/full mode');
 assert.ok(service.includes('request.IncludeDetails'), 'full mode still includes detailed rows');
 assert.ok(service.includes(': new List<CustomerComprehensiveItem>()'), 'summary mode does not fetch details');
 assert.ok(report.includes('@invoice.ItemCount قلم'), 'the group heading reads the actual item count');
 assert.ok(!report.includes('@invoice.Items.Count قلم'), 'summary mode cannot use the intentionally empty details collection');
});
