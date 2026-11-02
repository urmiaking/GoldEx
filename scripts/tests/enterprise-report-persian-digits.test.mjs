import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { runInNewContext } from 'node:vm';

const clientRoot = new URL('../../src/App/Client/GoldEx.Client/', import.meta.url);
const serverRoot = new URL('../../src/App/Server/GoldEx.Server/', import.meta.url);
const source = readFileSync(new URL('wwwroot/js/report-print.js', clientRoot), 'utf8');
const css = readFileSync(new URL('wwwroot/css/reports/report-enterprise.css', serverRoot), 'utf8');

function createRuntime(nodes) {
    const report = { label: 'print-root' };
    const document = {
        querySelector: selector => selector === '.gex-print-document' ? report : null,
        createTreeWalker(root, mode) {
            assert.equal(root, report);
            assert.equal(mode, 4);
            let index = 0;
            return { nextNode: () => nodes[index++] ?? null };
        }
    };
    const window = {};
    runInNewContext(source, { window, document, NodeFilter: { SHOW_TEXT: 4 } });
    return { window, document, report };
}

test('print localization converts dates, prices, weight, identifiers and Arabic digits', () => {
    const numbers = [
        ['1405/08/08 ساعت 21:45', '۱۴۰۵/۰۸/۰۸ ساعت ۲۱:۴۵'],
        ['1,234,567.890 تومان', '۱,۲۳۴,۵۶۷.۸۹۰ تومان'],
        ['750 عیار / 3.125 گرم', '۷۵۰ عیار / ۳.۱۲۵ گرم'],
        ['فاکتور #2409 - 09123456789', 'فاکتور #۲۴۰۹ - ۰۹۱۲۳۴۵۶۷۸۹'],
        ['مبلغ ١٢٣٤، کد ٢٤٩', 'مبلغ ۱۲۳۴، کد ۲۴۹']
    ];
    const nodes = numbers.map(([value]) => ({
        nodeValue: value,
        parentElement: { closest: () => null }
    }));
    const { window } = createRuntime(nodes);
    window.localizeGoldExReportDigits();
    assert.deepEqual(nodes.map(x => x.nodeValue), numbers.map(x => x[1]));
    window.localizeGoldExReportDigits();
    assert.deepEqual(nodes.map(x => x.nodeValue), numbers.map(x => x[1]), 'idempotent');
});

test('localization only changes visible print text, not script/style or attributes', () => {
    const nodes = [
        { nodeValue: 'GoldEx 2026', parentElement: { closest: () => null } },
        { nodeValue: '.selector{width:100px}', parentElement: { closest: () => ({ tagName: 'STYLE' }) } },
        { nodeValue: 'const id = 4;', parentElement: { closest: () => ({ tagName: 'SCRIPT' }) } }
    ];
    const { window, report } = createRuntime(nodes);
    const originalAttribute = { href: '/invoice/123456', title: 'بارکد 1234' };
    report.attributes = originalAttribute;
    window.localizeGoldExReportDigits(report);
    assert.equal(nodes[0].nodeValue, 'GoldEx ۲۰۲۶');
    assert.equal(nodes[1].nodeValue, '.selector{width:100px}');
    assert.equal(nodes[2].nodeValue, 'const id = 4;');
    assert.equal(report.attributes, originalAttribute);
    assert.match(source, /window\.localizeGoldExReportDigits\(\);/);
});

test('page margin counter uses a Persian numeric counter style', () => {
    assert.match(css, /@counter-style gex-persian-page/);
    assert.match(css, /symbols: '۰' '۱' '۲' '۳' '۴' '۵' '۶' '۷' '۸' '۹'/);
    assert.equal((css.match(/counter\(page, gex-persian-page\)/g) || []).length, 2);
    assert.equal((css.match(/counter\(pages, gex-persian-page\)/g) || []).length, 2);
});
