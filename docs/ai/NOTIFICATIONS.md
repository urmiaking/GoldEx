# Notifications and Web Push

## UI and service contract

Enterprise notifications use a native HTML dialog styled as MeetFlow's left notification drawer: 420px wide on desktop from 960px and full viewport width below 960px, full viewport height, flush to the physical left edge, with a light scrim and square edges. Native modality supplies focus containment and background inertness. One chronological list replaces the old new/archive tabs. Read/dismiss state belongs to the signed-in user, while the invoice reminder itself stays persisted so dismissing it does not recreate it. Existing global IsRead values are preserved for historical compatibility. The visible list contains the latest 100 notifications. The header count reflects unread items within that list.

INotificationService handles list/read/dismiss operations. IPushNotificationService has dual server/WASM implementations for configuration, browser registration/revocation and a test send to the caller's current device. The controller and UI require Administrators/Owners. Only the public VAPID key reaches clients.

## Generation and delivery

NotificationBackgroundService checks all active stores every minute. Each store receives a fresh scope and explicit StoreContext. InvoiceRepository loads payments and uses the unchanged TotalUnpaidAmount formula, selecting invoices whose due date has passed and whose unpaid amount is positive. Reminder creation runs in a serializable transaction to avoid concurrent generation. No accounting or posting logic is changed.

NotificationPushService reconciles newly-created unread reminders with subscriptions that existed when the reminder was created. It does not send historical reminders immediately upon enabling a new device. The persistent queue has a unique notification/subscription pair, a two-minute atomic lease, a one-day expiry and at most five attempts with exponential delay. HTTP 429/5xx and transport timeouts retry. HTTP 404/410 disable the device. Role/store access, invoice due date/balance, subscription validity and user read/dismiss status are checked again immediately before dispatch. Provider acceptance means the push provider accepted the request, not proof that an OS banner was displayed. Stable notification tags replace duplicates after ambiguous transport failures.

Push payloads contain a generic reminder and a same-origin invoice deep link, excluding customer names and balances. SMS remains a separate user action. The test endpoint only targets a subscription owned by the caller in the active store and coalesces repeated test clicks for a minute.

## Browser and session lifecycle

The existing enterprise development and published service workers both import notification-worker.js. It displays notifications and handles safe same-origin navigation on click. Existing caching and custom-domain restrictions remain unchanged. There is no second competing service worker.

Permission is requested from the enable button only. Already-granted subscriptions can restore on sign-in without another prompt. Device opt-out persists under GoldEx.PushDisabled. A changed VAPID key replaces the old browser subscription. Endpoints are browser-wide: their SHA-256 hash is globally unique, and signing into another account/store replaces the old record and its queue. Logout revokes that user's persisted subscriptions across stores; another device restores after a new signed-in mount. Browser capability, permission and network failures are shown in the panel and do not prevent in-app notifications.

NotificationPushRepository accounts for child service scopes created by GoldExComponentBase during SSR: scoped notification queries use the HTTP request's resolved store, falling back to the worker's context when no HTTP request exists. Unfiltered browser binding lookup and user logout revocation are explicit cross-store operations; all returned lists and dispatch claims remain store-scoped.

## Configuration

The user requested a new GoldEx VAPID pair in src/App/Server/GoldEx.Server/appsettings.json, under Notifications:Push (Enabled, Subject, PublicKey, PrivateKey). Keep the pair stable across restarts. PrivateKey remains server-only and must never be returned in DTOs or logs. The generated pair is separate from MeetFlow's keys. WebPush 1.0.13 follows MeetFlow's provider protocol. Named HttpClient GoldExWebPush disables redirects, and each send has a 20-second cancellation deadline. Only known HTTPS browser-provider hosts and valid P-256/auth keys are accepted.

Migration: 20261027050442_AddNotificationPushDelivery adds NotificationPushSubscriptions, NotificationDeliveries and NotificationUserStates; historical notification rows are retained. Normal enterprise startup applies migrations.

## Verification

- Node regression suite covers browser registration/opt-out/key replacement, native dialog cleanup and safe worker payload/click handling, plus the existing shell and appearance checks.
- NotificationPushTester covers user/store binding, invalid endpoints/keys, delivery outcomes, per-user read/dismiss, duplicate suppression and settled-invoice cancellation.
- NotificationPushSqlTester uses an isolated temporary LocalDB database to test actual SQL lease exclusivity/recovery, tenant filters, role/store membership, balance/due filtering and logout revocation; the temporary database is removed afterward. It also verifies the configured P-256 public/private pair matches without printing it.
- NotificationPushTransportTester exercises the real WebPush encryption/signature library with the configured keys against an in-memory HTTP provider, verifying 201/410/429/503/401 classification without sending test data to another person's device.
- Live signed-in IAB verification opened the native dialog and rendered existing notifications without an archive tab. The initial browser permission/registration attempt was inconclusive to automation, and the Chrome fallback was stopped by the computer-use URL-verification guard. The user subsequently explicitly confirmed receiving the real notification on 2026-10-27 («حله نوتیف گرفتم»). Treat this as human confirmation of actual device receipt, distinct from the automated mock-provider tests; notification-click navigation was covered by the worker test rather than a live OS-banner click.


Final verification: 23 server tests and 36 JavaScript regression checks passed on 2026-10-27. The real WebPush transport tests restored the test project's updated transitive dependencies before execution. EF reported no pending model changes; git diff --check passed. Device receipt was confirmed by the user, while live OS-banner click-through was not separately exercised.

Drawer follow-up: live geometry checks confirmed left/top = 0, 420×900 at a 1440×900 viewport and 360×640 at a 360×640 viewport, with no horizontal drawer overflow. Escape closed the native drawer and released scroll lock. The original viewport was restored. Both linked fingerprinted and plain appearance stylesheets serve the updated geometry; no build was needed for this CSS-only change.

Compilation follow-up: a live automatic subscription restore exposed MVC's positional-record validation rule. Required/MaxLength annotations now target the constructor parameters rather than generated record properties. NotificationPushModelValidationTester checks real MVC validation for registration and revocation records, including missing/oversized fields.


## Refresh registration regression

A live refresh of the signed-in browser restored a subscription whose provider host was jmt17.google.com. This is Chromium's official legacy staging GCM endpoint, documented in [Chromium push constants](https://chromium.googlesource.com/chromium/src/+/refs/tags/148.0.7734.4/components/push_messaging/push_messaging_constants.cc). The previous allowlist rejected it, and a message-only FluentValidation exception lost its message when the middleware serialized an empty Errors collection. The client then displayed the raw validation JSON.

The allowlist now accepts exactly HTTPS jmt17.google.com with the /fcm/send/ prefix and default port; no broad Google-domain wildcard was added, and endpoints remain unmodified. Push validation returns field-specific failures, while middleware preserves a message-only exception under a general error key instead of returning empty errors. Warnings retain reason codes and provider hostname only, excluding full endpoint tokens and keys. Regression coverage checks the legitimate host, lookalike hosts, unexpected paths/ports and the client-facing validation response contract.

Live fix verification: 29 notification tests passed; the rebuilt local host returned HTTP 200. Two consecutive signed-in reloads of `/calculator` restored the browser subscription successfully (the panel showed «اعلان این دستگاه فعال است»), with no snackbar messages, browser errors or server registration-rejection warnings. The calculator route was retained and the drawer closed afterward.
