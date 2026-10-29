# GoldEx Desktop AppBar, Route Trail and Jalali Clock

Use for desktop navigation/header work in GoldEx. Read root `AGENTS.md` before changes.

## Components

- `src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor`: top-level actions, desktop branding, a single custom `RouteBreadcrumbs`, and the left-aligned clock.
- `RouteBreadcrumbs.razor`: this is **not MudBreadcrumbs**. It owns a Persian route-name table for known `ClientRoutes` prefixes, location subscription, useful ancestor links, and safe handling of dynamic IDs. It unsubscribes on disposal.
- `Clock.razor`: uses `PersianCalendar` to construct weekday + day + full month + year, Persian digits, and a second line with local clock. No decorative calendar icon.
- `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css`: desktop >=960px route trail, legacy breadcrumb hiding, clock and action positioning, responsive desktop/tablet widths, reduced-motion support.
- Existing page-local `MudBreadcrumbs` are **retained for mobile**, but hidden in the enterprise `.goldex-page-container` at >=960px. This is intentional: the user has NOT authorized redesign of mobile breadcrumbs yet.

## Invariants

1. RTL desktop header flows logo at far right, then custom route trail with ample margin from GoldEx, then actions (Help, mode, notifications), then date/time at far left. The route trail must shrink/ellipsis rather than overlay actions/date. Compact at 960–1199px; full labels on spacious screens.
2. An empty Help wrapper must not occupy a flex slot and double the gap to AppearanceControls: at desktop use `.gex-topbar-help {display:contents}`; its actual button has `order:-1` so it appears to the **right** of dark mode in RTL. Tablet 600–959px may use display:contents but must preserve its action order. Mobile <=599px hides Help per the existing layout.
3. `RouteBreadcrumbs` updates on `NavigationManager.LocationChanged` (including browser back/forward), strips query/hash, avoids printing raw GUIDs/invoice numbers/unknown segments, shows a named current destination, and only generates ancestor links for navigable known prefix routes. Extend its labels whenever adding routes; never use MudBreadcrumbs in the desktop AppBar.
4. Full Persian calendar date is written from `PersianCalendar` and weekday mapping, e.g. «چهارشنبه ۸ اردیبهشت ۱۴۰۵». Clock is below the date and numerals are Persian. Keep the minute refresh timer correctly disposed and prefer user's local/Blazor runtime clock, without inventing calendar time shifts. The calendar icon is intentionally omitted for a clean header.
5. Preserve mobile drawer, mobile bottom bar, responsive brand, user permissions, notifications, active store and custom domain shell. Do not change page headers/titles, only relocate the desktop route navigation.
6. Tests: `node --test scripts/tests/appbar-route-trail.test.mjs`, full `scripts/tests/*.test.mjs`, `git diff --check`; `GoldEx.Client.Components.csproj --no-restore` was compiled successfully. CSS changes after #256 required scoping the old `liquid-dock.test.mjs` #256 CSS assertion to the #256 block (not all subsequently appended CSS).
7. GitHub issue #257, user-facing enterprise release 1.0.105. No push/deploy without authorization.

## RTL and body gap correction (#258)
- The custom desktop route separator uses U+2190 left arrow plus direction:ltr;unicode-bidi:isolate. Old left-angle bracket mirrored backward in RTL. The desktop content gap is 26px to match Dock-to-AppBar 26px; mobile remains 12px.
- Manual Release Notes now belong to authenticated NavMenu via ReleaseNotesMenuItem, including desktop More overflow. The Home page retains the automatic new-version modal but no manual button.
## Correction after #258: SVG chevron, not text arrow (#259)
- In custom RouteBreadcrumbs the separator is ShellIcon Name="chevron-left", using existing left-pointing SVG geometry. Do not use a plain Unicode arrow or mirrored bracket. CSS controls its size/direction without flipping SVG. Existing mobile MudBreadcrumbs remain unchanged.
## Mobile navigation revision (#263, supersedes old mobile-breadcrumb policy)
- Mobile <960px now displays a second RouteBreadcrumbs in .gex-mobile-route-bar underneath the fixed AppBar, aligned with page body; the existing page-local MudBreadcrumbs in mobile page markup are visually hidden. Include 46px in --gex-content-offset. Desktop AppBar still owns its original breadcrumb.
- The mobile wordmark is globally centered with absolute left:50%/translate(-50%), while hamburger right, dark mode, notification and Settings left remain interactive. Keep a small shrink breakpoint <=399px and Help route in mobile NavMenu; never sacrifice actions for the logo.
## Scope correction #264 (overrides #263 mobile redesign)
- Desktop: GoldEx brand centered in AppBar; breadcrumb in body-aligned .gex-desktop-route-bar below it; include 44px for route row in desktop --gex-content-offset. Keep clock and all actions.
- Mobile: KEEP the new #263 .gex-mobile-route-bar and its 46px content offset. Revert only unwanted mobile logo centering, nav layout, profile/license card and date filters. Do not remove new mobile breadcrumb again! Legacy page-local MudBreadcrumbs hidden visually.
## Final desktop AppBar placement clarification (#265 overrides #264)
- Desktop keeps the original one-row Breadcrumb INSIDE AppBar, immediately left of the right-aligned brand ICON; only WORDMARK is independently centered. Action/clock positions and original body vertical offset remain. Mobile maintains the approved second row underneath its original header. See AGENTS #265.