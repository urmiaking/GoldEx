# GoldEx Desktop Liquid Navigation Dock

Applies to GoldEx desktop workspace shell only. Read `AGENTS.md` first.

## Entry points
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor` owns both native mobile Drawer and desktop glass dock. It imports `liquid-dock.js`, attaches after first render and disposes correctly.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor` owns all auth-gated links, the async active-store switcher and external vitrine destination; **do not duplicate this route list in a second component**.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor` renders `gex-desktop-brand` in the header, plus unchanged mobile branding.
- `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css` contains the desktop-only liquid glass overrides and mobile unchanged `display:contents` fallback.
- `src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js` manages overflow, keyboard Escape, viewport/media changes and async rerenders.

## Invariants
1. Desktop is defined as >=960px. Collapsed rail is 64px glass with only icons. Individual links expand to a label-bearing pill using transform/width/opacity, no heavy JavaScript-driven animation. Right anchor remains fixed and the pill grows LEFT in RTL. On pointer exit it returns to narrow form; focus-visible provides keyboard access. Gold, other palettes and dark mode share semantic theme variables.
2. No scroll container in the dock. Calculate available height from `.gex-menu-scroll.clientHeight` minus dynamically mounted store picker height; 48px item + 4px gap. Hidden native anchors carry `data-dock-overflow="true"` only at desktop sizes. The More flyout contains cloned links whose click is delegated to the original DOM anchor to preserve Blazor navigation and authorization. Multiple overflowing links use columns rather than scroll. Watch nav viewport via MutationObserver for store/permissions changes, use ResizeObserver for height, and remove listeners/attributes when disposed.
3. More supports click, outside click, Escape and focus restoration. Maintain original account avatar, logout, store switching and license warnings. On mobile the More wrapper and panel use `display:contents`; the license must never remain inert or aria-hidden on mobile. The drawer and bottom nav remain as before.
4. Preserve screen reader labels and `prefers-reduced-motion` + `prefers-reduced-transparency` fallbacks. No arbitrary new routes or auth leaks from duplicate menu markup.
5. Test: `node --test scripts/tests/liquid-dock.test.mjs` and all Node UI tests, `node --check .../liquid-dock.js`, `git diff --check`. For structural Razor edits, build `GoldEx.Client.Components.csproj` if warranted. A static or unit test does not establish actual signed-in browser visual correctness.

## Issue
Initial implementation: GitHub #251, enterprise release 1.0.99.
