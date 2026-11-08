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
