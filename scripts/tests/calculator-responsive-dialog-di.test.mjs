import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const source = (path) => readFileSync(new URL('../../' + path, import.meta.url), 'utf8');

test('calculator server DI registers dialog service for BaseLayout prerender', () => {
    const server = source('src/Calculator/Server/GoldEx.Calculator.Server/DependencyInjection.cs');
    const layout = source('src/App/Client/GoldEx.Client.Components/Layouts/BaseLayout.razor');
    const host = source('src/App/Client/GoldEx.Client.Components/Components/Dialogs/ResponsiveDialogHost.razor');
    assert.match(layout, /ResponsiveDialogHost\s*\/>/);
    assert.match(host, /@inject ResponsiveDialogService Dialogs/);
    assert.match(server, /using GoldEx\.Client\.Components\.Services;/);
    const components = server.split('internal IServiceCollection AddComponents()')[1].split('internal IServiceCollection AddStorage(')[0];
    assert.match(components, /services\.AddScoped<ResponsiveDialogService>\(\);/);
    assert.match(server, /\.AddComponents\(\)/);
});

test('calculator WebAssembly host still registers shared client components', () => {
    const client = source('src/Calculator/Client/GoldEx.Calculator.Client/DependencyInjection.cs');
    const shared = source('src/App/Client/GoldEx.Client.Components/DependencyInjection.cs');
    assert.match(client, /\.AddClientComponents\(\)/);
    assert.match(shared, /services\.AddScoped<ResponsiveDialogService>\(\);/);
});
