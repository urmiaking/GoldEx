# Application appearance

The enterprise appearance refresh preserves Gold as the default and keeps the existing persisted palette keys. Application pages remain MudBlazor-based. The later explicit navigation migration adopts the native shell from MeetFlow and Mkx while preserving GoldEx render modes and theme services.

## Palettes and persistence

`GoldEx.Client.Components/Themes/ColorPalettes.cs` owns paired light/dark palettes and their Persian display names:

| Persisted key | Display name |
|---|---|
| Gold | طلایی |
| Blue | اقیانوس |
| Green | یشم |
| Purple | زنبق |
| Red | رز |
| Orange | کهربا |

Gold's complete `CreateGoldTheme` and base-theme factory were restored verbatim from the original revision after the user's visual review. This includes the original light/dark colors, vivid blue `#4A86FF`, green `#3DCB6C`, red `#FF3F5F`, bright gold `#C9A227`, original contrast text and secondary text, neutral default table/input lines, warm surfaces and 24px radius. Both the initial redesign and a partial restoration were rejected as less lively. Preserve Gold's original accents, semantic colors and typography. The later explicit request for Ocean-like card separation changes only the light background/surface pair described below. The five non-Gold palettes keep their redesigned colors, while animations and the native AppBar remain enabled for every palette. The native header and sidebar use the MeetFlow line icon family and neutral line tokens. Semantic colors inside application pages remain unchanged.

`ThemeService` uses the existing raw local-storage keys `SelectedPalette` and `IsDarkMode`. Choosing a palette preserves mode, and choosing mode preserves palette. Invalid palette names are ignored; blocked browser storage does not prevent a visual change.

## Gold surface polish

The user explicitly requested card shadows as clear as Ocean after finding the first softer-shadow attempt imperceptible. Gold therefore uses the same shared card and hover shadow declarations as Ocean in `app.css`, with no Gold-specific weakening. `CreateGoldTheme` now pairs white light surfaces (`#FFFFFF`) with a warm page background (`#F5F3EE`), following Ocean's white-card/background separation. This shared palette change also applies to Calculator. All other Gold palette values, the base theme and dark surfaces remain unchanged.

Enterprise `appearance.css` also retains a restrained highlight on enabled primary buttons and selected home tabs. The former Gold-only drawer styling has been replaced by the native workspace navigation. Public vitrine keeps its separate theme. Browser synchronization supplies root mode/palette attributes.

## Animation and render acknowledgement

`AppearanceControls.razor` supplies accessible native buttons and a native palette popover, reused in AppBar and Profile. Each palette preview shows both light and dark colors. `IsChanging` prevents overlapping changes across both instances.

The JS module is a Razor class-library asset at `_content/GoldEx.Client.Components/js/appearance.js`. Keep it in the shared component project: the standalone Calculator also uses the shared `BaseLayout` and `IThemeService`. Server-only placement would break its initial browser synchronization. The legacy synchronous `ToggleMode()` entry point remains for its existing toggler.

For explicit enterprise changes, the module starts one root View Transition and calls `ThemeService.ApplyAppearanceAsync` through a .NET object reference. The `OnAppearanceChanged` asynchronous event lets BaseLayout acknowledge `OnAfterRenderAsync` before capturing the new MudThemeProvider stylesheet. Do not replace this acknowledgement with an arbitrary delay or remount the route to force an update.

Dark mode expands from the source button; returning to light contracts the old dark snapshot. Palette changes expand the new snapshot in either mode. The 800ms clip animation retains its final clip until the browser removes the snapshots, then releases it. Reduced motion, missing View Transition support, hidden documents, detached triggers and rejected snapshots fall back to applying appearance without the reveal. Mode and palette initial synchronization does not animate.

## Native AppBar and navigation migration

The user explicitly requested replacing GoldEx's own AppBar and NavMenu with MeetFlow/Mkx navigation. The reference is the active `D:/source/Municipality/MeetFlow/src/Client/MeetFlow.Client/Layout/MainLayout.razor` and its `meetflow.css`, plus Mkx's `MainLayout`, `WorkspaceNavigation`, `UiIcon` and `navigation.js`. MeetFlow's separate AppBar/NavMenu component files are legacy and are not its active design.

`Layouts/MainLayout.razor` now uses native HTML for the shell. `Drawer.razor` renders a fixed RTL sidebar, `NavMenu.razor` uses native Blazor `NavLink`, `AppBar.razor` renders a native header, and `MobileNav.razor` supplies the native bottom bar. `ShellIcon.razor` ports MeetFlow's line SVG family (24px viewbox, 1.65 stroke) and adds GoldEx-specific glyphs. No third-party icon font or new UI package is introduced.

The sidebar contains branding, the store selector, new invoice action, all prior navigation destinations, licensing notices, identity/sign-out. Color selection has subsequently moved to the floating left-edge picker described below. The header contains date/time, dark/light mode, contextual help, unread notification indication and account avatar. The profile avatar now navigates directly to account management, where license, health, appearance and administrator logs controls are retained. Store loading still uses `GoldExComponentBase` and `IStoreService`; no financial or authorization behavior is changed. Existing authentication guards and custom-domain vitrine URL generation remain in place.

| Width | Sidebar | Header | Main horizontal padding |
|---|---|---|---|
| Through 650px | Modal, min(328px, 88vw) | 70px + top safe area | 0 (page-owned gutters) |
| Above 650px, below 960px | Modal, min(328px, 88vw) | 72px + top safe area | 0 (page-owned gutters) |
| 960px and above | Permanent, 238px | 82px + top safe area | 16px |

The latest user request raises the header to MeetFlow heights, superseding the initial compact 60px/56px navigation migration. The 12px content gap and restored mobile body width remain. `--gex-content-offset` and `--gex-table-layout-delta` automatically shorten all five responsive-table variants by the extra header space: 22px desktop, 16px tablet, 14px phone. Their original 56px baseline remains unchanged so each increase is subtracted exactly once; table bottom positions should remain stable at the same viewport. Live Customers-page comparison confirmed this: at 1440px, header 60→82px and table 544→522px (bottom unchanged at 785.4px); at 768px, header 56→72px and table 472→456px (bottom 704px); at 452px, header 56→70px and table 456→442px (bottom 680px). Document extra vertical scroll remained 0px in all three cases. The body is locked only while the mobile navigation modal is open. Do not hide body overflow globally.

The shared RCL `navigation.js`, adapted from Mkx, traps Tab/Shift+Tab in the mobile menu, makes the workspace and bottom bar inert, restores focus and previous inert values on close, and releases the modal when crossing to desktop or disposing the layout. Escape, backdrop and route changes close the menu. The menu releases its focus/inert state before opening the store selector dialog. Only the sidebar's menu list scrolls, keeping brand, create action and account visible.

Notifications are mounted at shell root instead of inside the header stacking context. The native notification button opens the Mkx-style native notification dialog and receives its unread count. Browser modality handles focus containment; closed dialog content cannot receive focus. The bottom bar sits below the header, menu scrim and notifications; authenticated destinations are Home, Calculator, Quick Create (raised central plus button), Customers and Invoices. Inventory is reachable from the sidebar. Guests see Home, Calculator, Login and About. The notification center now keeps read/dismiss state per user; existing SMS actions remain explicit.

Enterprise `appearance.css` is loaded after MudBlazor CSS and excluded from public vitrine pages. The standalone Calculator keeps its own layout and menus. The shared appearance module and legacy theme entry points remain compatible.

## Floating left-edge palette

The user explicitly removed palette choices from NavMenu and requested Mkx's floating picker. `PalettePicker.razor` is mounted once at enterprise MainLayout root, alongside the mobile bar and notifications, outside page animation/transform containers. `NavigationAppearance.razor` and its sidebar swatch CSS were removed. The existing profile appearance controls remain a secondary entry point.

The tab stays attached to physical `left: 0`, with Mkx's rounded edge shape and a connected SVG sheet that opens from the tab. Its six paired light/dark previews come from `ColorPalettes`; selection calls the existing `IThemeService.SetPaletteAsync` and shares `IsChanging`/render acknowledgement. No second theme system, palette keys or libraries are introduced. Reduced motion disables the sheet transition.

The shared RCL `js/palette-picker.js`, adapted from Mkx, supports mouse/pen dragging, touch dragging without page scrolling or ghost clicks, and ArrowUp/ArrowDown/Home/End keyboard positioning. A normalized fraction is persisted under `GoldEx.PickerPosition`; invalid values or blocked storage fall back safely. Header/bottom-nav bounds and the visual viewport keep the tab and sheet reachable after resizing or opening the keyboard. Route navigation closes the sheet; disposal clears pointer/touch, popover and viewport listeners. The picker remains visible on mobile, with no hide action depending on the removed NavMenu palette.

The floating tab sits below navigation/notification overlays. Opening mobile navigation closes its top-layer popover and makes the floating widget inert alongside the workspace. Public vitrine and standalone Calculator retain their independent shells.

## Mobile quick-create menu and wordmark

The center plus opens a native HTML `<dialog>` action sheet in `QuickCreateMenu.razor`, bound through RCL `js/quick-create.js`. The launcher does not use MudDialog. A full-width invoice action leads four smaller cards: New Customer, Manual Inventory Entry, Customer Transfer and Payment Voucher. Palette-based icon tints, a rounded surface, backdrop and reduced-motion-aware entrance are scoped to the sheet. Bottom-bar styles target immediate links/buttons so they cannot override the sheet's nested action cards.

The browser handles modal focus containment and background inertness. The module dismisses open palettes, exposes `aria-expanded`, locks scrolling only while open, and releases the modal before following an action. Backdrop click, close button, Escape, route change, switching to desktop and disposal all close it. Actions are shown to Administrators/Owners, matching destination authorization; other signed-in users retain a center account link.

Invoice and manual inventory actions navigate to existing pages. Customer, transfer and payment list pages accept a consumed-once `?create=true` command: after render they remove the command from the URL with history replacement, and only the subsequent clean-URL render invokes the existing create workflow. This two-stage handoff avoids MudDialogProvider closing a newly opened editor when navigation completes. Refresh/back does not reopen it, and a later click works again. Existing customer/payment/transfer MudDialog editor forms are reused; no data is saved merely by opening the native launcher or a form. Transfer creation awaits price-unit loading before constructing the editor model. Financial APIs, validations and posting logic remain intact.

The shared sidebar/mobile `ShellBrand` wordmark is **GoldEx** with the original `.brand-text` CSS and local **Cinzel Decorative** font. On narrow phones only the adjacent gem mark hides, preserving the wordmark and 44px controls.

## Verification

Run the JS regression checks:

```powershell
node --test scripts/tests/appearance.test.mjs scripts/tests/navigation.test.mjs scripts/tests/palette-picker.test.mjs scripts/tests/quick-create.test.mjs
```

The twenty-eight checks cover Persian palette-name encoding, appearance reveal/fallback behavior and initial synchronization, plus mobile navigation focus wrapping, original inert-state restoration, breakpoint changes and listener/scroll cleanup on disposal, closing the floating sheet before mobile navigation, palette drag/touch/cancellation behavior, keyboard bounds, persisted positions and blocked storage, plus native quick-create modality, immediate release before navigation, dismissal/resize and listener cleanup.

A subsequent encoding correction was verified in the compiled component assembly and the live served WASM asset, and both enterprise and Calculator hosts were restarted successfully. On Windows, read Persian source explicitly as UTF-8 (or UTF-8 with BOM detection); platform-default Python reads can corrupt C# display strings independently of Razor markup. Reload an already open browser after restarting to load the updated client assembly.

The original appearance-only pass used a temporary HTML harness with production appearance CSS/JS and palette values. The subsequent native navigation migration was verified in the actual signed-in GoldEx application at 360, 768, 960 and 1440px, including the Customers table at the desktop breakpoint. The checks confirmed no outer horizontal overflow, preserved mobile padding, permanent desktop navigation, modal mobile navigation, Tab/Shift+Tab wrapping, Escape/focus return, route-close behavior, store-selector opening without a lingering menu lock, account actions, notification opening/closing, and dark/light and palette switching. Original Gold/light settings and the Customers route were restored afterward.

The server project compiled successfully with zero warnings/errors and all 15 appearance/navigation regression checks passed. The running local host serves the migrated shell. Anonymous navigation retains its existing authentication guards in the source; it was not exercised by signing out of the user's session.

The floating picker follow-up compiled with zero warnings/errors and passed all 22 appearance/navigation/picker checks. Live verification on the inventory list confirmed color switching preserves light mode, a mouse drag moves the tab without scrolling the page, the moved position survives reload, keyboard arrows restore the original position, and mobile navigation closes/inerts the picker. The sheet remained within the viewport at 1440×900 and 360×640; the original Gold/light setting, normalized midpoint position and inventory URL/query were retained.

Quick-create follow-up: the server built with zero warnings/errors and all 28 JS regression checks passed. Live checks opened all five destination forms without saving data, repeated customer creation after canceling, confirmed transfer price units were loaded, checked native Escape dismissal/scroll release, verified the sheet fits 360×640, and confirmed the GoldEx/Cinzel wordmark in both mobile header and desktop sidebar. The original home route and default viewport were restored.


## Notification center redesign (2026-10-27)

The old MudDrawer/MudTabs notification UI and archive tab are removed. `Notifications.razor` now renders a native HTML dialog, following Mkx's summary/list/footer composition and the workspace's ShellIcon family. A device notification section offers explicit enable, disable and test controls with clear permission/transport feedback. Existing light/dark palette tokens supply the surface, text, borders and accent colors. At the user's follow-up request, the container matches MeetFlow's left notification drawer: 420px wide, capped at 100vw, flush against physical left/top/bottom with square edges and full 100dvh height. It uses a light scrim and a short horizontal entrance. Safe-area padding protects header/footer; the list scrolls independently, with whole-drawer scrolling on short landscape screens. Reduced motion disables its entrance animation.

`NotificationList` renders one dated list with unread badges, read/dismiss controls and preserved invoice-view/SMS actions. Read notifications remain in the same list; there is no archive. Native modality, Escape/backdrop handling, focus restoration and cleanup live in shared `js/notifications.js`. See [NOTIFICATIONS.md](./NOTIFICATIONS.md) for the server pipeline and verification limits.


## Direct profile navigation (2026-10-27)

The header avatar now links directly to the existing `/user-accounts` management page, including its profile card and security actions. The old profile popover, dismiss layer, code-behind and unused CSS were removed. Its appearance, license and health controls are retained in a collapsed «ظاهر و وضعیت برنامه» expansion on that page, rendered only on expansion; log access remains administrator-only, and sidebar sign-out remains available. Guest behavior still links to login. This is a client UI change; no backend/authentication rules were changed and no automatic build was run.


## Connected calculator tabs (2026-10-27)

The calculator's six tools now share a workspace frame based on MeetFlow's Organization tabs: line icons, readable labels, a primary-colored bottom indicator and a small selected-tool title/description area connecting the tabbar to its content. The user clarified that the tabs themselves must be native, so the former MudTabs/MudTabPanel implementation has been removed. Native buttons and hidden tabpanel containers provide ARIA relationships; shared calculator-tabs.js handles roving focus, manual keyboard activation and physical-direction overflow arrows. Only the selected tool component is instantiated, retaining the prior inactive-tool disposal behavior without initializing all six tools upfront.

Enterprise appearance.css scopes the styling to `.gex-calculator-*`. Only each tool's direct outer card loses its separate border/shadow, so the content joins the frame; nested result cards and existing editor/popover styles remain intact. Phone layouts use native horizontal scrolling and 44px physical left/right controls. Arrow clicks only scroll; they never change the selected tool. Edge states are measured from bounding rectangles, avoiding RTL scroll-offset ambiguity. Reduced motion removes the small selection/content animations. The public/authenticated visibility split, Smart Tray query aliases, help context and embedded Home calculator behavior remain unchanged. No calculation formulas or API logic changed.

Verification: checked installed MudBlazor 9.5.0 component metadata for TabContent, PrePanelContent and tab styling parameters, preserved all six tool components/auth boundaries in source, validated release JSON and checked the diff for whitespace errors. No automatic build or compiled-app visual verification was performed for these Razor/CSS-only changes, following AGENTS.md; the open running app needs recompilation to render the new markup.

The user subsequently requested compilation: GoldEx.Server built successfully with zero warnings/errors, including the direct profile link and calculator TabContent/PrePanelContent templates. The local https host was restarted from that build.

Live compiled verification confirmed the six tabs, selected-tool title/description changes, return from reverse calculation to the main calculator, and the direct profile-management link. A subscription DTO validation error found during reload was fixed and covered by MVC validation regression tests; all 24 notification server tests passed. The local calculator returned HTTP 200 and browser error logs were empty after restart.


Native-tab correction: the previous styling-only MudTabs approach did not meet the user's intent. It was replaced completely with native HTML/Blazor tabs, retaining the calculator inputs/results, auth split, help context, Home embedding and Smart Tray query aliases. Keyboard arrows move focus in RTL order; Enter/Space activate, and Home/End reach the boundaries. JS observers/listeners are disposed with the component. All 43 shell/appearance/notification/calculator JS tests passed, including seven native-tab interaction cases.


Native-tab live verification: GoldEx.Server compiled with zero warnings/errors and the restarted host returned HTTP 200. The live calculator rendered six native BUTTON tabs and no MudTabs wrapper. At 360px, the left/right controls moved the native horizontal viewport in the intended physical directions without changing the selected tool; at the left boundary, the final advanced-search tab was fully visible and only the left control was disabled. Returning right/Home restored the first tab and the opposite boundary state. Keyboard RTL arrows moved focus and Enter activated reverse calculation; returning to the main calculator worked. Only the active panel had child content. At 1440px all tabs fitted and both controls were hidden. No horizontal page overflow or browser errors were observed; the original viewport was restored.

Mobile notification follow-up: below the shell breakpoint of 960px, the drawer now spans the full viewport width without an outer side border. At 960px and above it remains 420px wide. This CSS-only adjustment preserves native modality, Push controls and full-height layout.

Live verification confirmed drawer widths of 360px at a 360px viewport, 768px at 768px, and 420px at 960px, all flush to the left edge. The default viewport and previously selected advanced-search tab were restored; the drawer was left open for review. Both linked/fingerprinted and plain CSS served the change without recompilation.

Calculator tab color follow-up: inactive labels and line icons now inherit theme primary text (near-black in light mode); only the selected tool uses the palette accent. Inactive hover uses the neutral page background rather than a primary tint. Dark mode retains readable neutral text through the existing text token.

Live color verification in Gold/light: selected label/icon were rgb(201,162,39); all five inactive labels/icons were rgb(28,37,38). Both linked/fingerprinted and plain CSS served the change without a build.
