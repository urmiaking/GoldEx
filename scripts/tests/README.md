# GoldEx maintained behavior test suite

The October 30, 2026 cleanup removed 18 static-only Node UI assertions and two mock DevExpress CSS/browser fixtures. This folder retains 13 behavior-oriented Node test files.

Run after relevant JavaScript behavior changes, from the project root:

    node --test scripts/tests/*.test.mjs

Tests exercise real JavaScript functions with controlled environment mocks: appearance state transitions, calculator RTL interactions, native navigation/focus/teardown, desktop Liquid Dock state, notification permissions/navigation, palette-picker gestures, mobile quick-create, responsive dialogs, invoice visibility/ResizeObserver lifecycle and DevExpress action callbacks/localization.

Do not add tests solely to verify a CSS selector, Razor attribute, or visual spacing. Simple UI changes need targeted source review and visual inspection in the actual app when feasible. Some mixed retained tests still have static guards; avoid expanding those.

The separate .NET unit/integration tests and financial/business logic tests must be kept. See AGENTS.md for the current test maintenance policy.
