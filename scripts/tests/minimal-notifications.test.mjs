import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const read = path => readFileSync(new URL('../../'+path, import.meta.url), 'utf8');
const dir='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const drawer=read(dir+'Notifications.razor');
const code=read(dir+'Notifications.razor.cs');
const list=read(dir+'NotificationList.razor');
const listCode=read(dir+'NotificationList.razor.cs');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

test('notification drawer is minimal, with unread count, read-all, and optional push setup',()=>{
  assert.match(drawer,/gex-notification-title">اعلان‌ها/);
  assert.match(drawer,/@if \(_unreadCount > 0\)[\s\S]*?class="gex-notification-unread-count"/);
  assert.match(drawer,/@if \(_unreadCount > 0\)[\s\S]*?HandleMarkAllAsRead/);
  assert.match(drawer,/<NotificationList Items="_items" Busy="_working" OnOpen="HandleOpenNotificationAsync" \/>/);
  for(const removed of ['TestPushAsync','DisablePushAsync','PushDescription','gex-notification-summary',
     'gex-notification-footer','gex-notification-list-toolbar','gex-notification-push-actions',
     'gex-notification-push-icon','تست اعلان','غیرفعال‌سازی','تازه‌سازی اعلان‌ها'])
    assert.ok(!drawer.includes(removed), 'obsolete UI: '+removed);
});

test('Push setup is conditional and stays hidden after subscription',()=>{
  assert.match(drawer,/_pushState\.Supported && _pushConfiguration\.Enabled && !_pushState\.Subscribed && _pushState\.Permission != "denied"/);
  assert.match(drawer,/class="gex-notification-enable-button"[^>]*@onclick="EnablePushAsync"/);
  assert.doesNotMatch(code,/private async Task TestPushAsync\(/);
  assert.doesNotMatch(code,/private async Task DisablePushAsync\(/);
  assert.match(code,/EnablePushAsync\(false\)/);
  assert.match(code,/_pushState = _pushState with \{ Subscribed = true \}/);
});

test('only unread notifications are loaded, badge/polling/read-all remain',()=>{
  assert.match(code,/_items = result\.Where\(x => !x\.IsRead\)\.ToList\(\); _unreadCount = _items\.Count;/);
  assert.match(code,/UnreadCountChanged\.InvokeAsync\(_unreadCount\)/);
  assert.match(code,/PeriodicTimer\(TimeSpan\.FromMinutes\(1\)\)/);
  assert.match(code,/MarkAllAsReadAsync\(ct\)/);
  assert.match(list,/aria-label="اعلان‌های خوانده‌نشده"/);
  assert.match(list,/اعلان خوانده‌نشده‌ای ندارید/);
});

test('notification is one keyboard-accessible click target; no per-item command buttons',()=>{
  assert.match(list,/<button type="button"\s+@key="notification.Id"/);
  assert.match(list,/@onclick="\(\) => OnOpen\.InvokeAsync\(notification\)"/);
  assert.match(listCode,/EventCallback<GetNotificationResponse> OnOpen/);
  assert.doesNotMatch(list,/OnMarkAsRead|OnDelete|OnButtonAction|notification\.Buttons/);
  assert.doesNotMatch(list,/<article|gex-notification-card-actions|gex-notification-dismiss/);
  assert.match(list,/class="gex-notification-item-preview"/);
  assert.match(css,/\.gex-notification-item-preview \{[\s\S]*?-webkit-line-clamp: 2/);
  assert.match(css,/\.gex-notification-item:focus-visible/);
});

test('clicking a notification marks read then navigates safely to its internal destination',()=>{
  assert.match(code,/private async Task HandleOpenNotificationAsync\(GetNotificationResponse notification\)/);
  assert.match(code,/service\.MarkAsReadAsync\(notification\.Id, cancellationToken\)/);
  assert.match(code,/_items\.RemoveAll\(x => x\.Id == notification\.Id\)/);
  assert.match(code,/UnreadCountChanged\.InvokeAsync\(_unreadCount\)/);
  assert.match(code,/await CloseAsync\(\);\s*Navigation\.NavigateTo\(target\)/);
  assert.match(code,/ButtonActionType\.NavigateToUrl/);
  assert.match(code,/url\.StartsWith\("\/", StringComparison\.Ordinal\)/);
  assert.match(code,/!url\.StartsWith\("\/\/", StringComparison\.Ordinal\)/);
  assert.match(code,/Uri\.TryCreate\(url, UriKind\.Relative/);
  assert.match(code,/ClientRoutes\.Invoices\.SetInvoice\.Replace/);
  assert.doesNotMatch(code,/SendReminderAsync|HandleButtonAction|HandleDelete\(/);
});

test('minimal list has touch-friendly density and theme-aware affordances',()=>{
  assert.match(css,/\.gex-notification-item \{[\s\S]*?min-width: 0;[\s\S]*?border-bottom: 1px solid var\(--mud-palette-lines-default\)/);
  assert.match(css,/\.gex-notification-item:hover \{ background: rgba\(var\(--mud-palette-primary-rgb\)/);
  assert.match(css,/\.gex-notification-unread-dot \{/);
  assert.match(css,/\.gex-notification-item-chevron/);
  for (const removed of ['.gex-notification-card {','.gex-notification-action {',
    '.gex-notification-list-toolbar {','.gex-notification-footer {','.gex-notification-new {'])
    assert.ok(!css.includes(removed),'orphaned selector '+removed);
});
