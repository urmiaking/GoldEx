import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
const root=resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const invoiceDetails=readFileSync(resolve(root,'src/App/Client/GoldEx.Client/Pages/Invoices/Components/InvoiceDetails.razor'),'utf8');

test('accounting and inventory timelines have no pagination controls',()=>{
 assert.doesNotMatch(invoiceDetails,/ChangeTransactionPage|ChangeInventoryPage|DetailPageSize|_transactionPage|_inventoryPage/);
 assert.doesNotMatch(invoiceDetails,/صفحه\s*@|>قبلی<\/button>|>بعدی<\/button>/);
 assert.match(invoiceDetails,/<ol class="invoice-event-timeline" aria-label="تراکنش‌های مالی فاکتور">/);
 assert.match(invoiceDetails,/<ol class="invoice-event-timeline" aria-label="رویدادهای گردش انبار فاکتور">/);
 assert.match(invoiceDetails,/<span>@_transactionTotal تراکنش<\/span>/);
 assert.match(invoiceDetails,/<span>@_inventoryTotal رویداد انبار<\/span>/);
});

test('both APIs fetch all invoice events in bounded batches before displaying a complete timeline',()=>{
 assert.match(invoiceDetails,/private const int TimelineBatchSize = 100;/);
 for(const name of ['GetTransactionResponse','GetInventoryStockItemResponse']){
  assert.match(invoiceDetails,new RegExp('var collected = new List<'+name+'>\\(\\);'));
 }
 for(const [method,field,total,failed] of [
  ['LoadTransactionsAsync','_transactions','_transactionTotal','_transactionsFailed'],
  ['LoadInventoryAsync','_inventory','_inventoryTotal','_inventoryFailed']
 ]){
  const a=invoiceDetails.indexOf('private async Task '+method+'()');
  const b=invoiceDetails.indexOf('private async Task ',a+16);
  const body=invoiceDetails.slice(a,b>0?b:undefined);
  assert.ok(a>=0);
  assert.match(body,/new RequestFilter\(offset, TimelineBatchSize\)/);
  assert.match(body,/offset \+= batch.Count;/);
  assert.match(body,/while \(offset < total\)/);
  assert.match(body,/batch.Count == 0 && offset < total/);
  assert.ok(body.includes(failed+' = true;'));
  assert.ok(body.includes(field+' = collected;'));
  assert.ok(body.includes(total+' = collected.Count;'));
 }
});

test('inventory stays lazy while retry and accounting balances remain',()=>{
 assert.match(invoiceDetails,/if \(tab == 1 && !_inventoryLoaded\) await LoadInventoryAsync\(\)/);
 assert.match(invoiceDetails,/@onclick="LoadTransactionsAsync">تلاش دوباره/);
 assert.match(invoiceDetails,/@onclick="LoadInventoryAsync">تلاش دوباره/);
 assert.match(invoiceDetails,/<CustomerRemaining CustomerId="@CustomerId"/);
});