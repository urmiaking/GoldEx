import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const read = (p) => readFileSync(new URL('../../' + p, import.meta.url), 'utf8');
const server = 'src/App/Server/GoldEx.Server.Application/Services/SmartTrayService.cs';
const tray = 'src/App/Client/GoldEx.Client/Pages/SmartTrays/SmartTrayPage.razor';
const trayLogic = 'src/App/Client/GoldEx.Client/Pages/SmartTrays/SmartTrayPage.razor.cs';
const customers = 'src/App/Client/GoldEx.Client/Pages/Customers/Components/';

test('a tray without a typed title gets the server-allocated number as its title', () => {
    const source = read(server);
    assert.match(source, /var trayNumber = .*?repository\.GetNextTrayNumberAsync\(cancellationToken\)/s);
    assert.match(source, /string\.IsNullOrWhiteSpace\(request\.Title\)\s*\?\s*\$"سینی شماره \{trayNumber\}"/);
    assert.match(source, /request\.Title\.Trim\(\)/);
    assert.doesNotMatch(source, /throw new InvalidOperationException\("عنوان سینی نمی‌تواند خالی باشد/);
});

test('new tray is created directly once, without dialog, and disables button while busy', () => {
    const source = read(trayLogic);
    const view = read(tray);
    assert.match(source, /private async Task CreateTrayAsync\(\)/);
    assert.match(source, /if \(_isCreatingTray\) return;/);
    assert.match(source, /CreateTrayAsync\(new CreateSmartTrayRequest\(\), ct\)/);
    assert.match(source, /await LoadSelectedTrayAsync\(tray\.Id\)/);
    assert.doesNotMatch(source, /ShowAsync<CreateSmartTrayDialog>/);
    assert.equal((view.match(/OnClick="CreateTrayAsync"/g) ?? []).length, 2);
    assert.equal((view.match(/Disabled="_isCreatingTray"/g) ?? []).length, 2);
    assert.match(view, /smart-tray-empty-state/);
});

test('both customer dialogs use the new responsive native modal and preserve filtered lists', () => {
    const caller = read(customers + 'CustomersList.razor.cs');
    const dialog = read(customers + 'CustomerHistoryDialog.razor');
    assert.equal((caller.match(/ResponsiveDialogs\.Show<CustomerHistoryDialog>/g) ?? []).length, 2);
    assert.doesNotMatch(caller, /DialogService\.ShowAsync<TransactionList>|DialogService\.ShowAsync<InvoicesList>/);
    assert.match(dialog, /class="gex-customer-history"/);
    assert.match(dialog, /<CustomerLedgerDirectory CustomerId="@CustomerId.Value"/);
    assert.match(dialog, /<InvoicesList CustomerId="@CustomerId"/);
    assert.match(dialog, /Session\?\.Cancel\(\)/);
});