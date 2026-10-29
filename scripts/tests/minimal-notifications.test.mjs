import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const read = path => readFileSync(new URL('../../'+path, import.meta.url), 'utf8');
const dir='src/App/Client/GoldEx.Client.Components/Layouts/Components/';
const notifications=read(dir+'Notifications.razor');
const code=read(dir+'Notifications.razor.cs');
const list=read(dir+'NotificationList.razor');
const css=read('src/App/Server/GoldEx.Server/wwwroot/css/appearance.css');

test('notification drawer contains only title, unread count, optional enable and unread list',()=>{
  assert.match(notifications,/gex-notification-title">اعلان‌ها/);
  assert.match(notifications,/@if \(_unreadCount > 0\)[\s\S]*?class="gex-notification-unread-count"/);
  assert.match(notifications,/@if \(_unreadCount > 0\)[\s\S]*?HandleMarkAllAsRead/);
  assert.match(notifications,/<NotificationList Items="_items"/);
  for(const removed of ['LoadNotificationsAsync"><ShellIcon Name="refresh"','TestPushAsync','DisablePushAsync','PushDescription',
    'gex-notification-summary','gex-notification-footer','gex-notification-list-toolbar','gex-notification-push-actions',
    'gex-notification-push-icon','تست اعلان','غیرفعال‌سازی','تازه‌سازی اعلان‌ها']) {
    assert.ok(!notifications.includes(removed), 'obsolete UI is still rendered: '+removed);
  }
});

test('push setup is brief, conditional, and disappears immediately once subscribed',()=>{
  assert.match(notifications,/_pushState\.Supported && _pushConfiguration\.Enabled && !_pushState\.Subscribed && _pushState\.Permission != "denied"/);
  assert.match(notifications,/class="gex-notification-enable"/);
  assert.match(notifications,/class="gex-notification-enable-button"[^>]*@onclick="EnablePushAsync"/);
  assert.match(notifications,/!string\.IsNullOrWhiteSpace\(_pushFeedback\) && !_pushState\.Subscribed/);
  assert.doesNotMatch(code,/private async Task TestPushAsync\(/);
  assert.doesNotMatch(code,/private async Task DisablePushAsync\(/);
  assert.match(code,/if \(_pushState\.Permission == "granted" && _pushConfiguration\.Enabled\)[\s\S]*?EnablePushAsync\(false\)/);
  assert.match(code,/_pushState = _pushState with \{ Subscribed = true \}/);
});

test('only unread notifications appear, but notification actions and badges stay functional',()=>{
  assert.match(code,/_items = result\.Where\(x => !x\.IsRead\)\.ToList\(\); _unreadCount = _items\.Count;/);
  assert.match(code,/UnreadCountChanged\.InvokeAsync\(_unreadCount\)/);
  assert.match(code,/PeriodicTimer\(TimeSpan\.FromMinutes\(1\)\)/);
  assert.match(code,/MarkAsReadAsync\(id, ct\)/);
  assert.match(code,/MarkAllAsReadAsync\(ct\)/);
  assert.match(list,/aria-label="اعلان‌های خوانده‌نشده"/);
  assert.match(list,/class="gex-notification-card is-unread"/);
  assert.doesNotMatch(list,/notification\.IsRead|gex-notification-new/);
  assert.match(list,/OnMarkAsRead\.InvokeAsync\(notification\.Id\)/);
  assert.match(list,/OnButtonAction\.InvokeAsync/);
  assert.match(list,/اعلان خوانده‌نشده‌ای ندارید/);
  assert.doesNotMatch(list,/وقتی خبری باشد/);
});

test('CSS removes dead banners and adds compact theme-aware actions',()=>{
  assert.match(css,/\.gex-notification-heading-actions \{/);
  assert.match(css,/\.gex-notification-unread-count \{/);
  assert.match(css,/\.gex-notification-mark-all \{/);
  assert.match(css,/\.gex-notification-enable-button \{/);
  assert.match(css,/\.gex-notification-enable-button \{[\s\S]*?color: var\(--mud-palette-primary-text\)/);
  for(const removed of ['.gex-notification-summary {','.gex-notification-push-actions {','.gex-notification-list-toolbar {',
    '.gex-notification-footer {','.gex-notification-new {']) assert.ok(!css.includes(removed),removed+' should have no CSS');
  assert.match(css,/@media \(max-width: 959\.98px\)[\s\S]*?\.gex-notifications \{ width: 100vw;/);
});
