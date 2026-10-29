# GoldEx Desktop Liquid Navigation Dock

Applies to GoldEx desktop workspace shell only. Read `AGENTS.md` first.

## Entry points
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor` owns both native mobile Drawer and desktop glass dock. It imports `liquid-dock.js`, attaches after first render and disposes correctly.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor` owns all auth-gated links, the async active-store switcher and external vitrine destination; **do not duplicate this route list in a second component**.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor` renders `gex-desktop-brand` in the header, plus unchanged mobile branding.
- `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css` contains the desktop-only liquid glass overrides and mobile unchanged `display:contents` fallback.
- `src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js` manages overflow, keyboard Escape, viewport/media changes and async rerenders.

## Invariants
1. Desktop is defined as >=960px. Collapsed rail is 64px glass with only icons. A SINGLE clipped glass background bends its actual outer edge left at the hovered/focused row; native links only expand hit areas and reveal labels, not separate glass islands. Right anchor remains fixed and the pill grows LEFT in RTL. On pointer exit it returns to narrow form; focus-visible provides keyboard access. Gold, other palettes and dark mode share semantic theme variables.
2. No scroll container in the dock. Calculate available height from `.gex-menu-scroll.clientHeight` minus dynamically mounted store picker height; 48px item + 4px gap. Hidden native anchors carry `data-dock-overflow="true"` only at desktop sizes. The More flyout contains cloned links whose click is delegated to the original DOM anchor to preserve Blazor navigation and authorization. Multiple overflowing links use columns rather than scroll. Watch nav viewport via MutationObserver for store/permissions changes, use ResizeObserver for height, and remove listeners/attributes when disposed.
3. More supports click, outside click, Escape and focus restoration. Maintain original account avatar, logout, store switching and license warnings. On mobile the More wrapper and panel use `display:contents`; the license must never remain inert or aria-hidden on mobile. The drawer and bottom nav remain as before.
4. Preserve screen reader labels and `prefers-reduced-motion` + `prefers-reduced-transparency` fallbacks. No arbitrary new routes or auth leaks from duplicate menu markup.
5. Test: `node --test scripts/tests/liquid-dock.test.mjs` and all Node UI tests, `node --check .../liquid-dock.js`, `git diff --check`. For structural Razor edits, build `GoldEx.Client.Components.csproj` if warranted. A static or unit test does not establish actual signed-in browser visual correctness.

## Issue
Initial implementation: GitHub #251, enterprise release 1.0.99.

## Seamless liquid-glass follow-up #252
- Superseded by #253: Drawer now contains one continuous gex-dock-glass-frame > gex-dock-glass, plus the separate active golden drop. liquid-dock.js updates clip-path for the ONE glass surface, never a second lobe.
- Preserve a one-piece account/logout hover and focus-within region. The logout text span is desktop-only and the absolute action begins immediately beside the avatar without pointer dead zone. Never require navigation to the account page to click logout.
- Active current store retains its real LogoUrl inside the collapsed circle, remains hoverable even for single-store users (clicking opens no list when only one store), and has enough chevron inline-end padding.
- The active indicator anticipates clicked internal NavLink destination and follows Blazor active-state updates; hidden active links map to More. External new-tab links must not move it. Respect reduced motion and keyboard accessibility. Tests cover these invariants.
## Single continuous dock glass (#253)
- Use ONE clipping surface, gex-dock-glass, behind all links. Its path() shape in liquid-dock.js is 272px wide; the normal right rail is x=208..272. At hover, the left boundary forms smooth joined shoulders in that row. Unlike #252, never overlay gex-dock-liquid-lobe; keep desktop aside background/shadow transparent so no second white card appears. CSS transitions interpolate identical path commands; reduced motion disables transition.
- The avatar/signout bottom case uses dockSilhouette(..., bottomRow=true). Its lower control points must coincide, giving a single connected bottom-left expanded corner, not a loop/double stripe. The background itself gets pale-red gex-dock-is-account tint and the whole logout link remains transparent; icon and text occupy the connected red area.
- Inspect static headless Chrome rendering at desktop width >=960, plus full UI Node tests and the clipped contour regression. Static mock screenshots do not replace a signed-in browser check. Keep mobile drawer and route droplet untouched.
## Content-fitted droplet + gold Create + account-only expansion (#254)
- One clipped glass contour retains a fully rounded ~23px left cap; no long flat cylinder. Measure actual label text with DOM Range (rather than CSS min-width 123px) and set a separate --gex-item-expanded-width for each native nav link. Keep sensible minimum/maximum values and recalculate after fonts load.
- New Invoice is its own Gold primary pill: only its own anchor expands, while the shared glass stays narrow. Do not convert it to the generic glass hover. Icon and text remain primary contrast color.
- Account/logout uses identity bounding rect and small shoulder, clamped H-52 instead of H-106, so More (...) above is never included in expansion. Sign-out remains one connected hit area, with width derived from its own Persian text. Keep mobile/keyboard/overflow/routing unchanged.
- scripts/tests/liquid-dock.test.mjs checks rounded content widths, Create exclusion and account height. Chrome headless static preview is not authenticated browser signoff.
## Selected item label contrast in other themes (#255)
- The selected icon belongs to the primary-colored moving droplet and should remain --mud-palette-primary-text (usually white). The expanded hover/focus **text** belongs to the translucent glass and needs --mud-palette-text-primary instead in all non-Gold palettes. Scope CSS to html:not([data-gex-palette="Gold"]) .goldex-shell .gex-liquid-dock .gex-navigation-menu > a.active:is(:hover,:focus-visible) > span at min-width 960px. This is not a filled active link in Ocean/Forest/etc, so never set its text to primary-text while the background is transparent.
- Preserve Gold's saturated active button/white text and mobile drawer. Chrome computed-style test at 1280px confirmed non-Gold light/dark hover-equivalent focus-visible label colors match the theme; selected icon remains white. Maintain the test in scripts/tests/liquid-dock.test.mjs.