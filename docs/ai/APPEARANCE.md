# Application appearance

The enterprise appearance refresh preserves Gold as the default and keeps the existing persisted palette keys. The implementation remains MudBlazor-based; it does not adopt the native control library or render-mode settings of Mkx.

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

Gold's complete `CreateGoldTheme` and base-theme factory were restored verbatim from the original revision after the user's visual review. This includes the original light/dark colors, vivid blue `#4A86FF`, green `#3DCB6C`, red `#FF3F5F`, bright gold `#C9A227`, original contrast text and secondary text, neutral default table/input lines, warm surfaces and 24px radius. Both the initial redesign and a partial restoration were rejected as less lively. Keep Gold's original appearance intact rather than changing individual colors again without a new user request. The five non-Gold palettes keep their redesigned colors, while animations and the compact AppBar remain enabled for every palette. AppBar icon styling preserves component-defined semantic colors, and its subtle bottom border uses the neutral line token rather than Gold's gold divider token.

`ThemeService` uses the existing raw local-storage keys `SelectedPalette` and `IsDarkMode`. Choosing a palette preserves mode, and choosing mode preserves palette. Invalid palette names are ignored; blocked browser storage does not prevent a visual change.

## Animation and render acknowledgement

`AppearanceControls.razor` supplies accessible native buttons and a native palette popover, reused in AppBar and Profile. Each palette preview shows both light and dark colors. `IsChanging` prevents overlapping changes across both instances.

The JS module is a Razor class-library asset at `_content/GoldEx.Client.Components/js/appearance.js`. Keep it in the shared component project: the standalone Calculator also uses the shared `BaseLayout` and `IThemeService`. Server-only placement would break its initial browser synchronization. The legacy synchronous `ToggleMode()` entry point remains for its existing toggler.

For explicit enterprise changes, the module starts one root View Transition and calls `ThemeService.ApplyAppearanceAsync` through a .NET object reference. The `OnAppearanceChanged` asynchronous event lets BaseLayout acknowledge `OnAfterRenderAsync` before capturing the new MudThemeProvider stylesheet. Do not replace this acknowledgement with an arbitrary delay or remount the route to force an update.

Dark mode expands from the source button; returning to light contracts the old dark snapshot. Palette changes expand the new snapshot in either mode. The 800ms clip animation retains its final clip until the browser removes the snapshots, then releases it. Reduced motion, missing View Transition support, hidden documents, detached triggers and rejected snapshots fall back to applying appearance without the reveal. Mode and palette initial synchronization does not animate.

## AppBar, body and tables

Enterprise-only `GoldEx.Server/wwwroot/css/appearance.css` is loaded after MudBlazor styles, outside public vitrine pages. MainLayout marks its MudLayout with `.goldex-shell`; geometry changes are scoped to that shell and do not change Calculator AppBar height.

| Width | AppBar height | Content gap below AppBar |
|---|---|---|
| Below 960px | 56px | 12px |
| 960px and above | 60px | 12px |

The earlier 82px desktop / 70px mobile AppBar with 24px / 16px gaps was reduced after the user's side-by-side visual review. Desktop top-bar controls use 40px boxes; mobile controls retain 44px touch targets.

Safe-area top padding is added to the AppBar and body offsets. Clipped drawers explicitly use the same height, overriding MudBlazor's breakpoint-dependent height fractions. Bottom navigation/notification stacking remains as documented in AGENTS.md. The mobile settings shortcut is available through navigation; the top settings shortcut is hidden below 600px to keep appearance and notification controls reachable.

`--gex-content-offset` is AppBar height plus safe-area top plus content gap. `--gex-table-layout-delta` subtracts the previous shell content origin, 56px (40px margin plus 16px padding), from the new offset. All five existing `.responsive-table*` container variants deduct that delta from their own original breakpoint-specific viewport calculation. Outside `.goldex-shell`, the delta remains zero. Dynamic viewport units and safe-area bottom are accounted for, with a 120px minimum table area on very short screens. Internal table scrolling remains available for long lists; the body is not globally locked or clipped to conceal overflow.

## Verification

Run the JS regression checks:

```powershell
node --test scripts/tests/appearance.test.mjs
```

The eleven checks cover Persian palette-name encoding, reveal direction, retaining clips through snapshot removal, preserving mode during palette changes, reduced motion, unsupported/rejected/interrupted snapshots, hidden/detached sources and initial browser synchronization.

A subsequent encoding correction was verified in the compiled component assembly and the live served WASM asset, and both enterprise and Calculator hosts were restarted successfully. On Windows, read Persian source explicitly as UTF-8 (or UTF-8 with BOM detection); platform-default Python reads can corrupt C# display strings independently of Razor markup. Reload an already open browser after restarting to load the updated client assembly.

During implementation, browser checks used a temporary HTML harness with production appearance CSS, the production JS module and palette values extracted from C#. This verified the native picker, light/dark changes, palette changes and a representative viewport-sized table at 360, 768, 960 and 1440px without outer vertical overflow. It is not a substitute for running the compiled Blazor app. No solution build was run, following the project's UI build policy. Final integration checks should cover persisted settings, both control instances, sidebar/notifications, license banners, route changes and real tables at desktop/mobile sizes.
