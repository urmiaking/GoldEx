# GoldEx responsive dialog motion
- MeetFlow's src/Client/MeetFlow.Client/Components/MfDialog.razor.css and native modal lifecycle are the reference.
- Mobile modal should fade and rise 18px with minimal scale rather than sliding 100% of a tall form. Avoid mobile backdrop-filter blur as it repaints a viewport; retain dim overlay.
- Update viewport geometry for actual resize/keyboard changes only. Never use per-scroll style writes. Preserve focus, Escape, non-dismissable backdrop, native showModal, stable overlay providers and nested forms.
- Run node --test scripts/tests/responsive-dialog.test.mjs and full JS suite. Authenticated browser visual proof is separate from source tests.

## Calculator SSR registration requirement
- `BaseLayout` hosts `ResponsiveDialogHost` for both enterprise GoldEx and Calculator. Calculator server prerender requires scoped `ResponsiveDialogService` registered in Calculator.Server `DependencyInjection.AddComponents`, while Calculator WASM gets it from `AddClientComponents()`. Missing server registration causes a fatal injection exception before interactive startup.
- Run `scripts/tests/calculator-responsive-dialog-di.test.mjs` and Calculator.Server Release build when shared dialog host injection changes.

## MudSelect/MudAutocomplete popup stacking
- Keep MudPopoverProvider in the native dialog top layer, outside clipped/animated .gex-modal-panel. The .gex-modal-overlays sibling should be z-index:5, pointer-transparent except popovers/snackbars, and display:contents when inactive.
- Do not override .mud-popover to position:fixed: MudBlazor positions fixed or absolute according to the component (MudSelect default PopoverFixed=false). Respect built-in FlipAlways and bound the dropdown list height.
- Regression: scripts/tests/responsive-dialog-popovers.test.mjs. Compile shared client components, calculator server, and enterprise server. Browser visual check is separate.

## Bounded selects (2026-11-03)
- Exception: only .gex-dialog-bound-popover inside active native dialogs uses fixed positioning with CSS variables computed by placeDialogPopovers. Do not alter global .mud-popover positioning.
- Shared JS observes popovers and constrains select/autocomplete lists to panel header/footer and available width, flips above near bottom, and limits to max 340px scrolling. Stop handlers on close.
- Test scripts/tests/responsive-dialog-popover-bounds.test.mjs and responsive-dialog lifecycle. Chrome geometry harness is ephemeral and should not be committed.

## Field-width dropdown alignment (2026-11-03)
- Only marked modal select/autocomplete popovers are custom positioned. Derive their exact width from their closest input's rect, not popup or panel width. Set explicit width and max-width CSS overrides; max-width alone is insufficient.
- Desktop dropdown defaults below its field; long options scroll. Mobile may flip upward. Test scripts/tests/responsive-dialog-popover-bounds.test.mjs, keep other popovers unchanged.

## Payment cash-account autocomplete on mobile (2026-11-04)
- PaymentEditor.razor uses MudAutocomplete rather than MudSelect for the invoice cash-payment bank-account field. Keep OpenOnFocus=true so touching an already populated field opens its options.
- SearchFinancialAccountsAsync returns all accounts for an empty query or the exact text of the currently selected account; typing a different query still filters. Otherwise focus can display only the already selected account.
- Regression: scripts/tests/responsive-dialog-popovers.test.mjs plus existing bound-popover tests. Source tests do not replace authenticated mobile browser verification.
## Autocomplete results behind mobile settlement dialog (2026-11-05)
- MudAutocomplete search data in PaymentEditor is already loaded; popup visibility depends on placeDialogPopovers in the shared responsive-dialog.js.
- A menu with a missing or out-of-wrapper MudPopover marker previously skipped positioning entirely. Resolve the marker's nearest .mud-select/.mud-autocomplete as usual, and for exactly one open popup fall back to the focused select/autocomplete inside the active modal panel, ignoring inert/hidden controls.
- Position and constrain menus to the actual visualViewport (software keyboard) intersected with the modal header/footer bounds. Keep the MudPopoverProvider inside native dialog overlay; never move provider DOM outside the top layer.
- Regression includes autocomplete-without-marker and keyboard-shrunken-viewport cases in scripts/tests/responsive-dialog-popover-bounds.test.mjs. Other popovers (e.g. date pickers) must remain unchanged. Confirm in an authenticated mobile browser after shipping.