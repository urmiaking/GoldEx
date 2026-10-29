# DevExpress GoldEx Report Viewer Styling

The GoldEx invoice viewer is `DxWasmDocumentViewer` from DevExpress Blazor JSBasedControls 24.2.7, NOT the newer native `DxReportViewer` with OnCustomizeToolbar. Official DevExpress technique: customize CSS palette classes prefixed by `.dx-designer-viewport .dxd-*`, per https://docs.devexpress.com/XtraReports/400413/web-reporting/asp-net-core-reporting/document-viewer-in-asp-net-applications/customization/customize-the-document-viewer-color-scheme .

- The invoice preview is `src/App/Client/GoldEx.Client/Pages/Invoices/Viewer.razor`, with an isolated `gex-report-viewer` wrapper. `src/App/Server/GoldEx.Server/Components/App.razor` links `wwwroot/css/devexpress-goldex.css` after appearance.css.
- All CSS selectors are scoped under `.gex-report-viewer`. Use CSS variables from existing MudBlazor palette, including surface, background, text, primary, primary-text and border, to support every GoldEx theme. Style the official dxd palette, toolbar, right tab panel and search/inputs; do not reimplement the viewer.
- Never override report document bricks, page contents, PDFs, or invoices themselves. Avoid `overflow:hidden` on the outer wrapper because export menus and overlays can be clipped. Keep print/zoom/export and `reportingCustomization.onBeforeRender` (zoom=0) unchanged. Do not edit package/vendor CSS.
- Test `node --test scripts/tests/devexpress-report-theme.test.mjs` and full regression suite. Headless Chrome fixture with DevExpress's actual 24.2.7 stylesheet confirmed distinct light/dark surfaces and the toolbar's glass blur.
- Running localhost may still serve an OLD compiled App.razor; a CSS file returning HTTP 200 alone does NOT mean it is referenced in the live document. After the next client/server build/restart, verify new stylesheet link and authenticated report UI.
- Release 1.0.117, GitHub issue #269. Commit to local Dev only unless the user requests push.
