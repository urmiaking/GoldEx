import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../../src/App/', import.meta.url);
const read = path => readFileSync(new URL(path, root), 'utf8');

test('vitrine visibility filter is nullable, API-bound and applied before SQL count/paging', () => {
  const contract = read('Shared/GoldEx.Shared/Enums/InventoryFilter.cs');
  const repository = read('Server/GoldEx.Server.Infrastructure/Repositories/InventoryStockRepository.cs');
  const directory = read('Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectory.razor.cs');
  assert.match(contract, /bool\? ShowInVitrine = null/);
  assert.match(repository, /inventoryFilter.ItemType == ItemType.Product && inventoryFilter.ShowInVitrine.HasValue/);
  assert.match(repository, /x\.Product!\.ShowInVitrine == inventoryFilter.ShowInVitrine.Value/);
  assert.ok(repository.indexOf('inventoryFilter.ShowInVitrine.HasValue') < repository.indexOf('var total = await groupedQuery.CountAsync'));
  assert.match(directory, /Name = "vitrine"/);
  assert.match(directory, /parameters.Add\("vitrine", _vitrineFilter.Value\)/);
  assert.match(directory, /ItemType == ItemType.Product \? _vitrineFilter : null/);
  assert.match(directory, /previousVitrine != _vitrineFilter/);
  assert.match(repository, /ShowInVitrine = null,/);
});
test('all, inside, outside vitrine options remain confined to Product with reset and URL persistence', () => {
  const dialog = read('Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectoryFiltersDialog.razor');
  const logic = read('Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectoryFiltersDialog.razor.cs');
  const directory = read('Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectory.razor.cs');
  assert.match(dialog, /@if \(_itemType is ItemType.Product\)[\s\S]*?Label="وضعیت ویترین"/);
  for(const text of ['همه اجناس','داخل ویترین','خارج از ویترین']) assert.ok(dialog.includes(text));
  assert.match(logic, /_vitrineFilter = null;/);
  assert.match(logic, /_itemType == ItemType.Product \? _vitrineFilter : null/);
  assert.match(directory, /_vitrineFilter.HasValue && ItemType == ItemType.Product/);
  assert.match(directory, /_vitrineFilter = ItemType is ItemType.Product \? result.VitrineFilter : null/);
});
test('inventory, invoice, customer date-range icons use a shared leading RTL adornment', () => {
  const css = read('Server/GoldEx.Server/wwwroot/css/appearance.css');
  for(const path of [
    'Client/GoldEx.Client/Pages/InventoryStocks/Components/InventoryDirectoryFiltersDialog.razor',
    'Client/GoldEx.Client/Pages/Customers/Components/CustomerFiltersDialog.razor',
    'Client/GoldEx.Client/Pages/Invoices/Components/InvoiceFiltersDialog.razor'
  ]){
    const markup = read(path);
    assert.match(markup, /MudDateRangePicker[\s\S]*?Adornment="Adornment.Start"[\s\S]*?gex-filter-date-range/);
  }
  assert.match(css, /\.gex-directory-filter-form \.gex-filter-date-range \.mud-input-root \{[\s\S]*?direction: rtl;/);
  assert.match(css, /\.gex-directory-filter-form \.gex-filter-date-range \.mud-input-adornment \{[\s\S]*?order: -1;/);
});
