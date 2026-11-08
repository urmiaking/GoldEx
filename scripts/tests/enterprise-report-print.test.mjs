import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';

const client = new URL('../../src/App/Client/GoldEx.Client/', import.meta.url);
const server = new URL('../../src/App/Server/GoldEx.Server/', import.meta.url);
const readClient = p => readFileSync(new URL(p, client), 'utf8');
const readServer = p => readFileSync(new URL(p, server), 'utf8');

test('all fifteen built-in print routes reuse one enterprise template', () => {
    const dir = 'Pages/Reporting/Pages/Prints/';
    const names = readdirSync(new URL(dir, client)).filter(x => x.endsWith('ReportPrint.razor'));
    assert.equal(names.length, 15);
    for (const name of names) {
        const source = readClient(dir + name);
        assert.match(source, /<ReportPrintBase /, name);
        assert.match(source, /ReportTitle=/, name);
        if (name === 'CustomerComprehensiveReportPrint.razor') {
            assert.match(source, /<CustomerComprehensiveDocument /, name);
            const document = readClient('Pages/Reporting/Components/CustomerComprehensiveDocument.razor');
            assert.match(document, /<table class="gex-customer-report-table">/, name);
        } else {
            assert.match(source, /<table class="table">/, name);
        }
        assert.match(source, /<script src=".*report-print.js.*"><\/script>/, name);
    }
});

test('landscape printing is selected only for wide report tables', () => {
    const dir = 'Pages/Reporting/Pages/Prints/';
    const names = readdirSync(new URL(dir, client)).filter(x => x.endsWith('ReportPrint.razor'));
    for (const name of names) {
        const source = readClient(dir + name);
        const head = source.match(/<thead[\s\S]*?<\/thead>/)?.[0] ?? '';
        const columns = (head.match(/<th\b/g) ?? []).length;
        assert.equal(source.includes('Landscape="true"'),
            columns >= 7 || name === 'CustomerComprehensiveReportPrint.razor',
            name + ': ' + columns + ' columns');
    }
});

test('print shell uses tenant settings, not fictitious store or financial information', () => {
    const source = readClient('Pages/Reporting/Components/ReportPrintBase.razor');
    assert.match(source, /ISettingService/);
    assert.match(source, /_settings\.InstitutionName/);
    assert.match(source, /_settings\.Address/);
    assert.match(source, /_settings\.PhoneNumber/);
    assert.match(source, /_settings\?\.HasIcon/);
    assert.match(source, /Summary\?\.Sections\.FirstOrDefault/);
    assert.match(source, /headline\.Items\.Take\(4\)/);
    assert.match(source, /<ReportSummary Summary="@Summary"/);
    assert.match(source, /@ChildContent/);
    assert.match(source, /ShowDateFilter/);
    assert.match(source, /_settingsLoaded/);
    assert.doesNotMatch(source, /Random\(|TotalSales\s*=|NetProfit\s*=/);
});

test('shared print stylesheet supports multipage A4, PDF, grayscale and repeated table headers', () => {
    const source = readServer('wwwroot/css/reports/report-enterprise.css');
    for (const fragment of ['@page {', '@page gex-landscape', 'size: A4 portrait', 'size: A4 landscape',
        'counter(page, gex-persian-page)', 'counter(pages, gex-persian-page)', '.gex-print-letterhead', '.gex-print-kpi-grid',
        '.gex-print-document .table thead', 'table-header-group',
        'page-break-inside:avoid', '.gex-print-summary-section',
        '.gex-print-document-footer', '@media print']) {
        assert.ok(source.includes(fragment), fragment);
    }
    assert.match(source, /Vazirmatn-font-face\.css/);
    assert.doesNotMatch(source, /https?:\/\//);
});

test('print opens after loaded fonts and logo, preserving popup close behavior', () => {
    const source = readClient('wwwroot/js/report-print.js');
    assert.match(source, /document\.fonts\?\.ready/);
    assert.match(source, /document\.images/);
    assert.match(source, /link\[rel="stylesheet"\]/);
    assert.match(source, /window\.addEventListener\('afterprint'/);
    assert.match(source, /window\.print\(\)/);
});