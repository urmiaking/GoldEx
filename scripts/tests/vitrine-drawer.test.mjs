import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "../..");
const vitrineCssPath = path.join(
  repoRoot,
  "src/App/Server/GoldEx.Server/wwwroot/assets/vitrine/css/vitrine.css"
);
const vitrineJsPath = path.join(
  repoRoot,
  "src/App/Server/GoldEx.Server/wwwroot/assets/vitrine/js/vitrine.js"
);
const vitrineCatalogPath = path.join(
  repoRoot,
  "src/App/Client/GoldEx.Client/Pages/Vitrine/VitrineCatalog.razor"
);

test("vitrine catalog filter drawer and backdrop have elevated z-index above header and mobile bar", () => {
  const css = fs.readFileSync(vitrineCssPath, "utf-8");

  // Header has z-index: 1000, mobile bar has z-index: 999
  assert.match(css, /\.vitrine-header\s*\{[^}]*z-index:\s*1000/s);
  assert.match(css, /\.vitrine-mobile-bar\s*\{[^}]*z-index:\s*999/s);

  // Backdrop and Drawer must be > 1000
  assert.match(css, /\.catalog-drawer-backdrop\s*\{[^}]*z-index:\s*1510/s);
  assert.match(css, /\.catalog-filter-drawer\s*\{[^}]*z-index:\s*1520/s);

  // Both should use 100dvh for modern mobile viewports
  assert.match(css, /\.catalog-drawer-backdrop\s*\{[^}]*height:\s*100dvh/s);
  assert.match(css, /\.catalog-filter-drawer\s*\{[^}]*height:\s*100dvh/s);
});

test("when drawer is open, main container is elevated above header & mobile bar and scroll is locked", () => {
  const css = fs.readFileSync(vitrineCssPath, "utf-8");

  // :has and body class elevate main above header/mobile nav
  assert.ok(
    css.includes(".vitrine-root:has(.catalog-filter-drawer.open) main") ||
    css.includes("body.vitrine-filter-drawer-open .vitrine-root main")
  );
  assert.match(
    css,
    /body\.vitrine-filter-drawer-open\s*\.vitrine-root\s*main,\s*\.vitrine-root:has\(\.catalog-filter-drawer\.open\)\s*main\s*\{[^}]*z-index:\s*1500/s
  );

  // Scroll lock on body
  assert.match(
    css,
    /body\.vitrine-filter-drawer-open,\s*\.vitrine-root:has\(\.catalog-filter-drawer\.open\)\s*\{[^}]*overflow:\s*hidden/s
  );
});

test("vitrine-page-content does not retain containing block or transform that traps fixed overlays", () => {
  const css = fs.readFileSync(vitrineCssPath, "utf-8");

  // vitrinePageReveal must settle to transform: none, not translateY(0)
  assert.match(css, /@keyframes vitrinePageReveal\s*\{[\s\S]*?to\s*\{[\s\S]*?transform:\s*none;/);

  // vitrine-page-content must not have permanent will-change: transform
  assert.match(css, /\.vitrine-page-content\.page-transition[^{]*\{[^}]*will-change:\s*opacity;/s);

  // Drawer & backdrop are excluded from section entrance animations
  assert.match(
    css,
    /\.vitrine-page-content\s*>\s*\*:not\(\.catalog-filter-drawer\):not\(\.catalog-drawer-backdrop\)/
  );
});

test("vitrine.js defines setFilterDrawerOpen and Escape listener", () => {
  const js = fs.readFileSync(vitrineJsPath, "utf-8");

  assert.ok(js.includes("setFilterDrawerOpen: function (isOpen)"));
  assert.ok(js.includes("document.body.classList.add(\"vitrine-filter-drawer-open\")"));
  assert.ok(js.includes("document.body.classList.remove(\"vitrine-filter-drawer-open\")"));
  assert.ok(js.includes("document.addEventListener(\"keydown\""));
});

test("VitrineCatalog renders drawer outside vitrine-container and syncs drawer state to JS", () => {
  const razor = fs.readFileSync(vitrineCatalogPath, "utf-8");

  // Rendered outside </main>\r\n</div>
  const mainCloseIdx = razor.indexOf("</main>");
  const drawerIdx = razor.indexOf("class=\"catalog-filter-drawer");
  assert.ok(drawerIdx > mainCloseIdx, "Filter drawer must be rendered outside main content container");

  // State synchronization
  assert.ok(razor.includes("goldexVitrine.setFilterDrawerOpen"));
  assert.ok(razor.includes("@inject IJSRuntime JsRuntime"));
});
