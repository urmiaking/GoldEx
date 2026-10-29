# GoldEx notification inbox — MeetFlow-style

Applicable to enterprise GoldEx notification drawer changes. Read AGENTS.md and the docs/ai guides first.

## Key files
- src/App/Client/GoldEx.Client.Components/Layouts/Components/NotificationList.razor and .razor.cs
- src/App/Client/GoldEx.Client.Components/Layouts/Components/Notifications.razor and .razor.cs
- src/App/Server/GoldEx.Server/wwwroot/css/appearance.css
- src/App/Shared/GoldEx.Shared/DTOs/Notifications/GetNotificationResponse.cs
- scripts/tests/minimal-notifications.test.mjs and notification-calculator-typography.test.mjs

## UX and behavior
- As of issue #262, the unread inbox follows MeetFlow's notification-item pattern: each notification is one full-row native button, not a card with multiple actions.
- Show compact icon, title (500 weight), at most two lines of secondary message, timestamp and a subtle unread dot/left chevron. No SMS, View, Read or Delete sub-buttons; no huge cards. Keep count/read-all/close in the header and the short optional Push activation row before subscription.
- On click, invoke EventCallback<GetNotificationResponse>. The parent calls INotificationService.MarkAsReadAsync, removes the unread item, updates the AppBar count, closes the drawer and navigates if a valid internal target exists.
- Prefer ButtonActionType.NavigateToUrl only when a safe root-relative in-app path, not an absolute URL. Otherwise use InvoiceId to generate the invoice route. If neither, only mark read. Never send a reminder SMS implicitly on row click.
- Preserve backend DTO/buttons for other clients, auto-polling, store-scoped list, unread filtering, native modal focus, Escape handling, Mark All and push subscription.
- Use theme variables in appearance.css, native focus-visible, 170ms hover and reduced-motion. Avoid 700/800 text and oversized cards.

## Verification
- Full Node suite: node --test scripts/tests/*.test.mjs (120/120 passed for #262).
- GoldEx.Client.Components.csproj --no-restore built with 0 warnings/errors.
- Release 1.0.110; local Dev commit only until user explicitly authorizes push/deploy.
