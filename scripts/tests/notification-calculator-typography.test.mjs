import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const css = readFileSync(new URL('../../src/App/Server/GoldEx.Server/wwwroot/css/appearance.css',import.meta.url),'utf8');
function declaration(selector) {
  const needle = selector+' {';
  const start = css.indexOf(needle);
  assert.ok(start >= 0,'Missing declaration: '+selector);
  return css.slice(start+needle.length,css.indexOf('}',start));
}
function weight(selector,expected) {
  assert.match(declaration(selector),new RegExp('font-weight:\\s*'+expected+'\\s*;'),selector+' weight '+expected);
}
test('minimal notification drawer uses restrained 400/500/600 typography',()=>{
  weight('.gex-notification-heading h2',600);
  weight('.gex-notification-mark-all',500);
  weight('.gex-notification-enable',400);
  weight('.gex-notification-enable-button',500);
  weight('.gex-notification-action',500);
  weight('.gex-notification-action--quiet',400);
  weight('.gex-notification-card h3',500);
  weight('.gex-notification-card p',400);
  weight('.gex-notification-card time',400);
  weight('.gex-notification-empty p',400);
  weight('.gex-notification-unread-count',500);
});
test('unread colors and activation actions use theme primary contrast, not heavy fonts',()=>{
  assert.match(declaration('.gex-notification-card.is-unread'),/border-color:\s*rgba\(var\(--mud-palette-primary-rgb\)/);
  assert.match(declaration('.gex-notification-card.is-unread'),/background:\s*rgba\(var\(--mud-palette-primary-rgb\)/);
  assert.match(declaration('.gex-notification-action'),/color:\s*var\(--mud-palette-primary-text\)/);
  assert.match(declaration('.gex-notification-enable-button'),/color:\s*var\(--mud-palette-primary-text\)/);
  assert.match(declaration('.gex-notification-heading h2'),/font-size:\s*19px/);
  assert.match(declaration('.gex-notification-card p'),/line-height:\s*1\.85/);
});
test('calculator tabs and context keep standard 400/500 typography',()=>{
  weight('.gex-calculator-tab',400);
  weight('.gex-calculator-tab.is-active',500);
  weight('.gex-calculator-context h2',500);
  weight('.gex-calculator-context p',400);
  assert.match(declaration('.gex-calculator-tab'),/font-size:\s*14px/);
  assert.match(declaration('.gex-calculator-context p'),/font-size:\s*13px/);
  assert.match(declaration('.gex-calculator-tab.is-active'),/color:\s*var\(--mud-palette-primary\)/);
  assert.match(declaration('.gex-calculator-tab.is-active::after'),/background:\s*var\(--mud-palette-primary\)/);
});
test('mobile calculator typography remains readable and tab interactions untouched',()=>{
  assert.match(css,/@media \(max-width: 650px\)[\s\S]*?\.gex-calculator-tab\s*\{[^}]*font-size:\s*13px/);
  assert.match(css,/\.gex-calculator-panels\s*>\s*\[role=tabpanel\]/);
  assert.match(css,/@media \(prefers-reduced-motion: reduce\)[\s\S]*?\.gex-calculator-tab\.is-active::after/);
});
