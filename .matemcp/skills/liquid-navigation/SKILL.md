# GoldEx Desktop Liquid Navigation Dock

Applies to GoldEx desktop workspace shell only. Read `AGENTS.md` first.

## Entry points
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/Drawer.razor` owns both native mobile Drawer and desktop glass dock. It imports `liquid-dock.js`, attaches after first render and disposes correctly.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor` owns all auth-gated links, the async active-store switcher and external vitrine destination; **do not duplicate this route list in a second component**.
- `src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor` renders `gex-desktop-brand` in the header, plus unchanged mobile branding.
- `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css` contains the desktop-only liquid glass overrides and mobile unchanged `display:contents` fallback.
- `src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js` manages overflow, keyboard Escape, viewport/media changes and async rerenders.

## Invariants
1. Desktop is defined as >=960px. Collapsed rail is 64px glass with only icons. A single shared decorative glass lobe stretches from the dock into the hovered or focused row; anchors only expand hit areas and fade labels, not create separate glass islands. Right anchor remains fixed and the pill grows LEFT in RTL. On pointer exit it returns to narrow form; focus-visible provides keyboard access. Gold, other palettes and dark mode share semantic theme variables.
2. No scroll container in the dock. Calculate available height from `.gex-menu-scroll.clientHeight` minus dynamically mounted store picker height; 48px item + 4px gap. Hidden native anchors carry `data-dock-overflow="true"` only at desktop sizes. The More flyout contains cloned links whose click is delegated to the original DOM anchor to preserve Blazor navigation and authorization. Multiple overflowing links use columns rather than scroll. Watch nav viewport via MutationObserver for store/permissions changes, use ResizeObserver for height, and remove listeners/attributes when disposed.
3. More supports click, outside click, Escape and focus restoration. Maintain original account avatar, logout, store switching and license warnings. On mobile the More wrapper and panel use `display:contents`; the license must never remain inert or aria-hidden on mobile. The drawer and bottom nav remain as before.
4. Preserve screen reader labels and `prefers-reduced-motion` + `prefers-reduced-transparency` fallbacks. No arbitrary new routes or auth leaks from duplicate menu markup.
5. Test: `node --test scripts/tests/liquid-dock.test.mjs` and all Node UI tests, `node --check .../liquid-dock.js`, `git diff --check`. For structural Razor edits, build `GoldEx.Client.Components.csproj` if warranted. A static or unit test does not establish actual signed-in browser visual correctness.

## Issue
Initial implementation: GitHub #251, enterprise release 1.0.99.

## Seamless liquid-glass follow-up #252
- The shared lobe and active drop are direct Drawer children. JS positions them with --gex-lobe-top, --gex-lobe-width and --gex-drop-top. Keep per-anchor hover background, shadows and glass blur disabled: only the shared lobe deforms beside the hovered or focused link.
- Preserve a one-piece account/logout hover and focus-within region. The logout text span is desktop-only and the absolute action begins immediately beside the avatar without pointer dead zone. Never require navigation to the account page to click logout.
- Active current store retains its real LogoUrl inside the collapsed circle, remains hoverable even for single-store users (clicking opens no list when only one store), and has enough chevron inline-end padding.
- The active indicator anticipates clicked internal NavLink destination and follows Blazor active-state updates; hidden active links map to More. External new-tab links must not move it. Respect reduced motion and keyboard accessibility. Tests cover these invariants.