# GoldEx AI Agent Instructions

Read these documents before generating code:

- ./docs/ai/ARCHITECTURE.md
- ./docs/ai/SERVICE_ARCHITECTURE.md
- ./docs/ai/DOMAIN_KNOWLEDGE.md
- ./docs/ai/DEVELOPMENT_GUIDE.md
- ./docs/ai/TOOLS.md

Every time that you learn something new about the project update the AGENTS.md file with the new information.
This file should be the single source of truth for all AI agents working on the project. Always refer to this file before generating code or making architectural decisions.
- **AUTOMATIC VERSIONING & RELEASE NOTES**:
  - **Scope**: `src/App/Server/GoldEx.Server/releases.json` belongs **exclusively to the main enterprise GoldEx project** (`src/App/`). Do **NOT** add or modify entries in `releases.json` for changes made to `GoldEx.Calculator` (`src/Calculator/`) or standalone SDK libraries.
  - **End-User Friendly & Non-Technical Language**: All change descriptions in `releases.json` **MUST** be written in very simple, plain, and non-technical Persian (فارسی روان، ساده و کاملاً غیرفنی برای کاربر نهایی و زرگرها). Strictly avoid technical developer jargon (such as SSR, WASM, PersistentComponentState, Cache-Control, DI, EF Core, etc.); instead, describe the change from the end-user's perspective and the practical benefit (e.g., «رفع مشکل نمایش فاکتور در موبایل»، «بهبود سرعت باز شدن صفحات»، «نمایش زنده تغییرات قیمت»).
  - **Execution**: Whenever you complete a feature implementation or fix a bug in the main GoldEx application, add a new release entry (or append to current version changes) in `src/App/Server/GoldEx.Server/releases.json`. Increment the version number (SemVer), set the release date (`yyyy-MM-dd`), and add a bulleted array of simple, user-facing Persian descriptions.

### AI Build Execution Policy
- **Do NOT automatically run `dotnet build`** or launch background solution builds after minor UI layout, Razor markup, CSS, styling, or markdown documentation edits.
- Only run `dotnet build` when introducing structural C# backend changes, adding new API endpoints/aggregates, making architectural refactorings, or when specifically requested by the user.

## Project Overview
GoldEx is a modern jewelry store management, accounting, and gold trading platform for gold/jewelry stores built with .NET 10, Blazor Web App, MudBlazor, and Domain-Driven Design (DDD).

The solution contains:
- GoldEx: Main enterprise web application
- GoldEx Mini: Offline-first (PWA) calculator and invoice application
- Shared SDK libraries
- Server-side APIs and infrastructure services

---

## Core Business Domain

The project operates in the gold and jewelry industry and includes:

- Gold inventory management
- Gold/Jewelry/MoltenGold/UsedGold sales and purchases
- Scrap gold and melting workflows
- Multi-currency accounting
- Double-entry accounting
- Real-time market pricing
- Barcode scanning and product tracking
- Customer ledger and settlement management

AI agents must preserve business correctness around:
- Gold weight precision
- Currency conversion accuracy
- Accounting integrity
- Inventory consistency
- Financial transaction safety

---

## Architecture Rules

Follow Clean Architecture and DDD strictly.

Dependency direction:

Server -> Application -> Infrastructure -> Domain

Rules:
- Domain layer must not reference EF Core, ASP.NET Core, MudBlazor, or infrastructure libraries.
- Application layer orchestrates use cases and validation.
- Infrastructure implements persistence and external services.
- UI logic belongs in client projects and server projects that has statically written (e.g. login).
- Shared DTOs belong in Shared projects.

---

## Important Projects

### Server
- GoldEx.Server (Server entry point, server-side components, controllers, report files, Dockerfile, DI bootstrap and so on)
- GoldEx.Server.Application (services, background services, mapper configs, validation and so on)
- GoldEx.Server.Domain (DDD style aggregates deriving from EntityBase and value objects inside each aggregate folder)
- GoldEx.Server.Infrastructure (Domain EF Configuration, migrations, external services, repositories in a generic repository style, specifications, DbContext)

### Client
- GoldEx.Client (Client side Pages and Components)
- GoldEx.Client.Components (Reusable components, layouts, themes, client-only services and so on)
- GoldEx.Client.Services (HttpClient implementation of Shared services located in GoldEx.Shared)

### Calculator
- GoldEx.Calculator.Client (Client side pages and components)
- GoldEx.Calculator.Server (Server entry point, controllers and so on)

### Shared
- GoldEx.Shared (Shared services interfaces, routes, DTOs, Enums and so on)

### SDK
- GoldEx.Sdk.Client
- GoldEx.Sdk.Common (Core framework codes used across client and server projects)
- GoldEx.Sdk.Server (Core framework codes used across server only projects)

---

## Coding Guidelines

### Backend
- Prefer async/await everywhere
- Use repository abstractions from Domain
- Use specification pattern for queries
- Keep business logic inside aggregates/domain services
- Avoid fat controllers
- Prefer strongly typed value objects

### Frontend
- Use MudBlazor components
- Keep components reusable
- Minimize code-behind complexity
- Inherit and use GoldExComponentBase methods in every component that needs api interaction
- Support responsive layouts

### Database
- Use EF Core configurations
- Avoid business logic in DbContext
- Preserve transactional consistency
- Never bypass domain invariants

---

## Financial Safety Requirements

Never:
- Use floating point for monetary precision
- Ignore rounding rules
- Change accounting logic casually
- Break inventory balance consistency
- Modify invoice calculation formulas without validation

Prefer:
- decimal types
- explicit precision handling
- audited calculations
- deterministic formulas

---

## Recommended AI Tasks

AI agents are encouraged to:
- Generate CRUD scaffolding
- Create DTO mappings
- Generate validators
- Refactor reusable components
- Improve architecture consistency
- Generate documentation
- Optimize LINQ queries
- Improve MudBlazor UI structure

AI agents should avoid:
- Altering accounting formulas without context
- Breaking layer boundaries
- Introducing hidden coupling
- Mixing infrastructure into domain models

---

## Multi-Tenancy Architecture (Shared Database)

GoldEx has transitioned to a shared database multi-tenancy model based on the `Store` aggregate root and the `IStoreFiltered` interface.

For full architectural details, scoping rules, global filter translation requirements, unique indexes, and asset resolution rules, refer to [ARCHITECTURE.md](./docs/ai/ARCHITECTURE.md#multi-tenancy-architecture-shared-database).

### Store Management Safety, Cloning & File Transitions

When working with stores and multi-tenancy assets:
1. **Default Store Safety**: The default store `Guid.Empty` (with slug `default`) represents historical data and must **never** be deleted.
2. **Configuration Cloning**: Creating a store via `CreateStoreAsync` automatically copies settings (`Setting`, `BarcodePrintSettings`, `PositionItems`), `SmsTemplate`s, system `LedgerAccount`s, and system `FinancialAccount`s from the default store to the new store in a database transaction, and copies default logo and report files.
3. **Asset Renaming on Slug Update**: Modifying a store's slug in `UpdateStoreAsync` automatically renames the app logo (`logo_{oldSlug}.png` -> `logo_{newSlug}.png`) in `uploads/icons/app/` and all related reports (`*_{oldSlug}.repx` -> `*_{newSlug}.repx`) in `Reports/`.
4. **Global Price System**: `PriceUnit`s, `Price`s, and `PriceHistory` are system-wide (global) and are shared across all stores. They do not implement `IStoreFiltered` and do not contain `StoreId`.
5. **Asset Deletion on Store Delete**: Deleting a store via `DeleteStoreAsync` automatically deletes its app logo (`logo_{slug}.png`) from `uploads/icons/app/` and all associated report files (`*_{slug}.repx`) from `Reports/`.
6. **FluentValidation Delegated Validations**: Validations for store creation, updates, and deletion must be handled via FluentValidation validators (`CreateStoreRequestValidator`, `UpdateStoreRequestValidator`, and `DeleteStoreValidator`) in the Application layer, rather than inline inside the service methods.
7. **Path Resolution**: Use `WebHostEnvironmentExtensions` extension methods to resolve path names for logos, reports, and other web host assets instead of manual path combinations.

---

## Licensing Architecture (Hybrid Model)

GoldEx supports a hybrid licensing system designed for multi-tenant and multi-store environments:

1. **Licensing Modes**: Configured in `appsettings.json` under `"License:Mode"`, supporting `"Hybrid"` (master instance license + local tenant subscriptions) or `"InstanceWide"` (single global license).
2. **Master Instance License**: The default store (`Guid.Empty`) registers remotely via `VHDLicenseManager` using the deployment domain name. This master license is periodically verified remotely.
3. **Tenant Store Subscriptions (Local)**: Individual stores/tenants are registered and tracked locally within the database (via `AppLicense` properties `Plan`, `ExpireDate`, and `RegisteredAt`, which implement `IStoreFiltered` to be tenant-scoped).
4. **Scoped Verification & Caching**:
   - `ProductLicense` is registered as a `Scoped` service to represent the active request's store license.
   - An in-memory thread-safe `ILicenseCache` (Singleton) stores resolved store licenses to prevent database query overhead on every request.
   - `LicenseResolutionMiddleware` runs after `StoreResolutionMiddleware` to determine the target `StoreId` based on the licensing mode, retrieve/cache the license bypassing tenant filters via `IgnoreQueryFilters()`, and populate the scoped `ProductLicense`.
5. **Validation & Expiration**:
    - `LicenseUpdaterBackgroundService` runs in the background to sync the master license remotely and evaluate tenant subscriptions locally against their expiration dates.
    - `CreateStoreRequestValidator` enforces active store counts against the license's `MaxStores` limit in `InstanceWide` mode.

---

## GoldEx Calculator Storage & Printing Architecture

GoldEx Calculator (`GoldEx.Calculator.Client`) is an offline-first client-side tool (PWA/Wasm) designed to manage invoices and store profiles independently of the backend database.

### 1. Local Storage Management
- All profile settings, invoice drafts, and generated invoice histories are persisted in the browser's `localStorage` via the `Blazored.LocalStorage` library.
- **LocalStorage Keys**:
  - `QuickInvoiceCompanyInfo`: Stores the shop's profile (name, phone, address, and the Base64-encoded store logo).
  - `QuickInvoiceBasket`: Stores current active invoice items in the basket before finalization.
  - `QuickInvoiceList`: Stores the history of generated invoices.
- **Store Logo Size Limits**: Because `localStorage` is subject to a 5MB browser quota, the store logo is limited to a maximum size of **512 KB** upon upload to prevent quota exhaustion.

### 2. Invoice Print System
- Print rendering is implemented entirely in client-side JavaScript (`wwwroot/quick-invoice.js`) within the `quickInvoice.printFromPayload` routine.
- **Layout & Style**:
  - When an invoice is printed, a new browser window is spawned, and the invoice HTML is written on the fly.
  - The styling is defined in `wwwroot/assets/css/quick-invoice.css`, configured specifically for **A5 landscape** printing (`@page { size: A5 landscape; margin: 8mm; }`).
  - If the store has uploaded a logo, it is embedded as a Base64 data URL directly in the print template's header (`.qi-header .qi-title`).

---

## Standalone Customer Transfer Voucher Architecture (حواله بین مشتریان)

GoldEx supports standalone customer-to-customer remittances (`CustomerTransferVoucher` aggregate) for both currency and gold weight (18K/Mesghal) transfers:

1. **Aggregate Root**: `CustomerTransferVoucher` (in `GoldEx.Server.Domain/CustomerTransferVoucherAggregate`) implements `IStoreFiltered`.
2. **Double-Entry Accounting**:
   - `AccountingTransactionService.CreateTransactionsForCustomerTransferVoucherAsync` generates balanced journal entries within a single `GroupId` (UUID v7).
   - Credits the source customer's sub-ledger (reduces store receivable from source customer).
   - Debits the destination customer's sub-ledger (reduces store payable to destination customer).
3. **Optional Invoice Settlement Linking**:
   - `CustomerTransferVoucherService` links optional `SourceInvoiceId` and `DestinationInvoiceId`.
   - Automatically creates linked `InvoicePayment` records to settle the open balances (`Remaining`) of selected source/destination invoices.
4. **UI & UX Standard**:
5. **Invoice-Level Customer Transfers (حواله در فاکتور)**:
   - When registering a payment of type `PaymentType.CustomerTransfer` on an invoice (e.g. Purchase Invoice paying via another customer's receivable), both legs of the balanced transaction entry must have `Transaction.InvoiceId = invoice.Id` (the source invoice).
   - Setting `Transaction.InvoiceId` to `payment.TargetInvoiceId` must never be done, as `ReplaceTransactionsForInvoiceAsync` on the target invoice will consider those transactions as removed and falsely generate reversal transactions.

---

## Sales Invoice Gold Weight Equivalent Reporting (معادل وزنی فاکتورهای فروش)

GoldEx calculates 18K gold weight equivalents (گرم طلای ۱۸ عیار / ۷۵۰) for monetary sales invoice reports (`SellInvoiceRpResponse`):

1. **Item Gold Conversion**:
   - For currency invoices (e.g. Toman, USD), each product item's financial components (`ItemProfitAmount`, `ItemWageAmount`, `ItemTaxAmount`, `ItemFinalAmount`) are converted to 18K gold weight using item base gold rate (`GramPrice`):
     - `ProfitWeight = ItemProfitAmount / GramPrice`
     - `WageWeight = ItemWageAmount / GramPrice`
     - `TaxWeight = ItemTaxAmount / GramPrice`
     - `ItemFinalWeight = ItemFinalAmount / GramPrice`
2. **Effective Rate & Invoice Adjustments**:
   - Invoice effective gold rate $\text{EffectiveGoldRate} = \frac{\sum \text{ItemFinalAmount}}{\sum \text{ItemFinalWeight}}$.
   - Discounts and extra costs are converted using $\text{EffectiveGoldRate}$.
   - Remaining balance weight equivalent $\text{RemainingWeight} = \frac{\text{TotalUnpaidAmount}}{\text{EffectiveGoldRate}}$.
3. **Gold-Based Invoices**:
   - For invoices where `PriceUnit.IsGoldBased` is true, amounts are already in grams and used directly.
4. **UI & Print Summary Integration**:
   - `SellInvoiceSummary.razor` and `SellInvoiceReportPrint.razor.cs` display a dedicated card/section titled **«معادل وزنی (گرم ۱۸)»** alongside currency summaries.

---

## Customer Running Balance in Invoice List (مانده کل حساب مشتری در لیست فاکتورها)

GoldEx displays each customer's running balance immediately after an invoice directly within the `InvoicesList` table rows:

1. **Async Performance Pattern**:
   - Similar to `CustomersList.razor`, `InvoicesList.razor` uses the `<CustomerRemaining>` component in each table row to load customer running balances asynchronously without slowing down the initial server-side query for the invoice table.
2. **Point-In-Time Balance Query**:
   - `ITransactionService.GetCustomerRemainingListAsync` accepts an optional `DateTime? untilDate`.
   - In `InvoicesList`, `UntilDate` is computed as `invoiceDate.ToDateTime(TimeOnly.FromTimeSpan(createdAt.TimeOfDay)).AddSeconds(1)`.
   - `TransactionRepository.GetCustomerRemainingListAsync` filters ledger transactions where `PostingDate < UntilDate`, accumulating all preceding transactions and those posted by the invoice itself, while excluding subsequent transactions.
3. **Multi-Unit Price Support**:
   - Displays running balances across all price units (currency, 18K gold, etc.) with automatic sliding carousel animation and manual slide toggle support.

---

## Executive Desktop Navigation & Home Page Architecture (پیش‌خوان و منوی بالای دسکتاپ)

GoldEx uses a high-performance executive layout in desktop mode (`>= 960px`):

1. **Antigravity-Style Desktop Appbar Top Menu**:
   - `AppBar.razor` includes a top navigation bar (`.desktop-top-nav`) featuring direct 1-click access buttons and clean dropdown menus (`MudMenu`) for **فاکتورها**, **انبار و اجناس**, **امور مالی**, and **ابزارها و گزارشات**.
   - Allows instant navigation without opening the sidebar drawer on desktop viewports.
2. **Lazy-Loaded View Rendering**:
   - On `Index.razor`, heavy table components (`InvoicesList`, `InventoryStockList`, `CustomersList`, `CheckPaymentsList`) are unmounted when inactive and rendered conditionally via `@if (_activePanelIndex == N)`.
   - Reduces initial DOM nodes by ~80% and eliminates MudBlazor tab-switching stutter.
3. **Custom Segmented Tab Controller & Smooth CSS Entrance Animations**:
   - Replaces default heavy MudBlazor tabs with `.goldex-segmented-container` pill tabs.
   - Hardware-accelerated CSS animations (`.animate-fade-in-up`) provide smooth 60fps view transitions.

---

## Mobile Navigation and Notification Layering

- Below `960px`, `Drawer.razor` renders a dedicated `MudOverlay` (`mobile-navigation-backdrop`) while the navigation drawer is open. Its black scrim has 60% opacity and sits at `--mud-zindex-drawer - 1`, below the drawer and top AppBar. Clicking it closes the drawer through `IsDrawerOpenChanged`, keeping the layout state synchronized.
- `MobileNav.razor` uses `mobile-navigation-bar` at `--mud-zindex-drawer - 2` on mobile, so both drawer backdrops and the notification panel cover the bottom navigation.
- `Notifications` is rendered inside the top AppBar's stacking context. Raising only the notification drawer's own z-index cannot place it above a sibling bottom AppBar with the same stacking level; keep the bottom navigation below that context.
- `.notification-drawer` fills the viewport using `100vh` with a `100dvh` override, rather than subtracting the bottom navigation's 80px height. Desktop notification width remains 400px.

---

## Model Context Protocol (MCP) & AI Integration Architecture (اتصال هوش مصنوعی و کلیدهای دسترسی)

1. **Multi-Tenancy, OAuth 2.0 & PAT Authentication**:
   - **Dual Authentication**:
     - **OAuth 2.0 (RFC 6749, RFC 7636 PKCE S256)**: Full authorization code flow for web-based AI clients (Google Gemini, ChatGPT Custom GPTs, Claude Web) with interactive Persian consent UI at `/oauth/authorize`, automatic client registration (RFC 7591) at `/oauth/register`, and standard token exchange at `/oauth/token`.
     - **Personal Access Tokens (PAT)**: Direct `Bearer gex_pat_...` or `X-API-Key` for IDEs (Cursor, Antigravity, Windsurf) and local scripts.
   - **Discovery Metadata**:
     - `GET /.well-known/oauth-protected-resource` (RFC 9728): Advertises authorization servers and supported scopes.
     - `GET /.well-known/oauth-authorization-server` & `GET /.well-known/openid-configuration` (RFC 8414): Full server metadata for automated client handshake.
   - `ApiKeyAuthenticationMiddleware` authenticates both OAuth Bearer tokens and PAT keys (SHA-256 hash), establishing the `ClaimsPrincipal`.
   - `StoreResolutionMiddleware` automatically resolves the active store (`StoreUser`), ensuring all MCP operations are tenant-scoped via EF Core global query filters (`IStoreFiltered`).
2. **Endpoints**:
   - `POST /mcp`: JSON-RPC 2.0 HTTP endpoint for standard MCP tool discovery and execution. Returns `401 Unauthorized` with `WWW-Authenticate` header pointing to `/.well-known/oauth-protected-resource` when unauthenticated.
   - `GET /mcp` (or `/api/mcp/sse`): SSE transport for persistent real-time tool sessions.
3. **Persian-First Tool Suite**:
   - `get_live_gold_prices`: Real-time market prices for gold, coins, and foreign currencies with units and update timestamps.
   - `calculate_gold_product_price`: Full invoice-ready gold pricing formula with dynamic price units, wages, seller profit, and tax.
   - `calculate_scrap_gold_valuation`: Scrap gold valuation with 750 deduction formulas.
   - `calculate_molten_gold`: Molten gold weight and price estimations.
   - `search_inventory_stock`: In-stock query across products, coins, and currencies.
   - `get_customer_balance`: Multi-currency running ledger balances for customers.
   - `search_customers`: Customer discovery by name or query.
   - `get_customer_statement`: Customer ledger transactions with running balance and price units.
   - `search_invoices` & `get_invoice_details`: Comprehensive invoice discovery and product breakdown with dynamic invoice price units (no hardcoded currency).
   - `get_trial_balance_report`: Accounting trial balance report.
   - `get_used_gold_hidden_profit`: Melting batch hidden profit and assay valuation reports.
4. **UI & Guided Setup**:
   - `PersonalAccessTokens.razor` under Settings (`ClientRoutes.Settings.PersonalAccessTokens = "/base-info/api-tokens"`).
   - Responsive UI (`@layout SettingsLayout`) with expansion panels for Google Gemini/ChatGPT (1-click OAuth), Cursor/Antigravity `mcp_config.json`, Claude Desktop `claude_desktop_config.json`, and Persian prompt cheat sheet with 1-click clipboard copy.
   - One-time reveal modal (`RevealTokenDialog.razor`) displaying the raw secret token with 1-click clipboard copy and security alerts.

---

## Online Showcase & Gallery Website Architecture (ویترین آنلاین طلا و معرفی گالری)

GoldEx provides a public-facing, responsive online showcase and digital catalog for gold and jewelry stores:

1. **Routing & Multi-Tenancy Resolution**:
   - Each store's showcase is accessible via `/{storeSlug}` (e.g. `/fani-jewelry`, `/tabriz-gold`, `/default`).
   - Vitrine subroutes:
     - `/{storeSlug}`: Store homepage, featured items, category highlights, live gold price ticker.
     - `/{storeSlug}/catalog`: Full product catalog with live search, category pills, and gold/jewelry filters.
     - `/{storeSlug}/p/{barcode}`: Product detail with multi-image gallery, real-time live price breakdown (raw gold value + wage/profit), gemstone details, and 1-click WhatsApp/Bale customer inquiry.
     - `/{storeSlug}/about`: Store introduction, address, contact phone, and social links.
   - For anonymous visitor queries, EF Core global query filters (`IStoreFiltered`) are bypassed with `.IgnoreQueryFilters()` and filtered explicitly by the store's `Id` resolved from `storeSlug`.
2. **Product Rules & Invariants**:
   - **Allowed Product Types**: Only `ProductType.Gold` and `ProductType.Jewelry` can be published to the vitrine (`ShowInVitrine`). `UsedGold` and `MoltenGold` are strictly prohibited by domain invariants.
   - **Image Management**: Supports multiple images per product (`ProductImage`), with 1 marked as `IsMain`. Uploaded files are saved to `uploads/products/` with unique UUID v7 filenames.
3. **Store Profile & Social Settings**:
   - Social links (`InstagramUrl`, `TelegramUrl`, `BaleUrl`, `WhatsAppNumber`) and `AboutText` are stored in `Setting` to avoid altering the `Store` schema.
   - Editable in admin panel under `/settings` («اطلاعات ویترین آنلاین و راه‌های ارتباطی»).
4. **UI/UX & Styling Standard**:
   - Custom luxury obsidian-and-gold theme defined in `wwwroot/assets/vitrine/css/vitrine.css`.
   - Lightweight gallery and share interaction helpers in `wwwroot/assets/vitrine/js/vitrine.js`.
   - Fast, fluid 60fps animations, mobile-first responsive layout, and distinct Drawer shortcut for store admins.
5. **Custom Domain & Public Vitrine Link Generation**:
   - Stores support optional `CustomDomain` (configured strictly by Administrators in `/settings/stores`).
   - `VitrineUrlHelper` generates public vitrine product and catalog links using `CustomDomain` (e.g. `https://fanijewellery.ir/{slug}/p/{barcode}`), falling back to current base URL if unconfigured.
   - Inventory management (`VitrineQuickEditDialog` and `InventoryStockList`) includes 1-click clipboard copy and open buttons for public product URLs.
6. **WebAssembly Bootstrap Footprint & Embedded Browser Compatibility**:
   - The public vitrine currently uses `InteractiveWebAssemblyRenderMode(prerender: true)` from the same `GoldEx.Client` project as the full administration application; it is not a separately trimmed vitrine client.
   - A representative .NET 10 release publish contains roughly 396 files and about 51.5 MB of uncompressed `_framework` assets, including large administration-only dependencies such as DevExpress and EF Core. This can make first-load hydration fragile in memory-constrained embedded browsers such as Instagram's in-app WebView even though SSR content remains visible.
   - The application currently sends `Cross-Origin-Embedder-Policy: require-corp` and `Cross-Origin-Opener-Policy: same-origin-allow-popups` on vitrine HTML and framework assets. Instagram's in-app WebView was confirmed to lose all Blazor interactivity in both `InteractiveWebAssembly` and `InteractiveServer` modes while these headers were present; ordinary mobile Chrome was unaffected. Hiding both response headers for the public custom domain in Nginx immediately restored interactivity.
   - Public vitrine routes must therefore remain exempt from COEP/COOP unless a future feature demonstrably requires cross-origin isolation and has been tested in embedded browsers. The WASM payload size remains a secondary first-load performance risk, not the cause of this confirmed Instagram failure.
   - Vitrine startup must unregister active service workers and delete `blazor-cache-*` Cache Storage entries **before** calling `Blazor.start()`. Cleanup after startup is too late: a stale WASM bundle can hydrate over correct SSR markup and display obsolete UI or DTO behavior.
   - **Multi-Layer Cache & Hydration Management**:
     1. `App.razor` executes an asynchronous pre-boot routine before `Blazor.start()`:
        - Unregisters all service workers on Vitrine routes. If a Service Worker controller is active at load time, it wipes `caches` and triggers a safe, one-time reload (guarded by `sessionStorage.getItem('gex_sw_cleared_once')`).
        - Enforces version-based cache eviction: Compares server version (`@AppVersion` from `IAppReleaseService` / `releases.json`) with `localStorage('gex_vitrine_wasm_ver')`. If different, it wipes all browser Cache Storage entries (`caches.delete()`) before boot.
        - Configures `Blazor.start({ webAssembly: { loadBootResource: ... } })` so that `blazor.boot.json` is always fetched with `{ cache: 'no-store' }`, guaranteeing that hash comparisons always reflect the latest deployment.
     2. `service-worker.published.js` actively self-destructs on non-platform custom domains (e.g. `fanijewellery.ir`), and treats `blazor.boot.json` and boot scripts as Network-First on platform domains.
     3. Vitrine static assets (`vitrine.css?v=@AppVersion` and `vitrine.js?v=@AppVersion`) are strictly version-stamped, and `vitrine.js` is loaded prior to `Blazor.start()`.
7. **Catalog "Newest" Sorting**:
   - The `VitrineCatalog.razor` `newest` option uses `Product.CreatedAt` as its chronology field. `GetVitrineProductsAsync` propagates it through `VitrineProductRawProjection` and `VitrineProductSummaryDto`.
   - For the `newest` option, `CreatedAt` descending is the primary ordering key. Availability is only a secondary tie-breaker, followed by `Id` descending for deterministic ordering. Do not place availability before `CreatedAt`, because that makes older in-stock products appear ahead of genuinely newer sold products while the UI says «جدیدترین».
8. **Vitrine Wage Unit & Conversion**:
   - The public vitrine reads the product's current configured wage (`Product.Wage`, `WageType`, and `WagePriceUnitId`). Purchase and sale invoice workflows retain their own wage snapshots, while the product remains the source of the current public/catalog wage.
   - Fixed wages must display `Product.WagePriceUnit.Title`; they must never be labeled with a hard-coded currency title. Percentage wages remain unitless percentages.
   - Vitrine prices are always displayed in Toman, independently of the system default price unit. Before calculating the estimated catalog price, a fixed wage uses its linked `Price.PriceHistory.CurrentValue` (stored in Rial), divides it by `10` to obtain the live Toman rate, and applies `Wage × LiveTomanRate × Weight`. `UnitType.TMN` uses rate `1`, while `UnitType.IRR` uses rate `0.1`.
   - Legacy price units whose `PriceId` is missing may resolve the live price by an exact `Price.Title` match. A missing non-base live rate must never silently fall back to `1`, because that would treat a foreign-currency wage as Toman.
   - Vitrine price breakdowns must use the current store's `Setting.GoldProfitPercent`, `Setting.JewelryProfitPercent`, and `Setting.TaxPercent` and the shared `CalculatorHelper.Product` formulas. Do not hard-code 7% profit or 9% tax: jewelry commonly uses a different configured profit percentage, and tax settings can change.

9. **Partial Sale State in Public Vitrine**:
   - Product.Weight is the original/full product weight; the current public remaining weight comes from the net InventoryStock balance.
   - A vitrine product is partially sold when its positive remaining stock is lower than Product.Weight (using a small decimal tolerance).
   - Public DTOs expose OriginalWeight, RemainingWeight, and IsPartiallySold. Catalog, home/search, product detail, WhatsApp inquiry, and share/story content must explicitly say that weight and price refer to the remaining portion so a partially sold set is never presented as a complete set.

10. **Custom-Domain Deployment Cache Checks**:
   - Vitrine pages prerender on the server and then rerender from `GoldEx.Client` WebAssembly. An element present in SSR but absent after hydration can indicate an older client bundle or a client-side rendering difference; compare the actual HTML, loaded framework asset URLs, and browser network responses before changing component markup.
   - On `fanijewellery.ir`, Nginx has separate locations for framework scripts, WebAssembly binaries, static files, and HTML. Only its framework-script location hides COEP/COOP; the general HTML and binary locations can still pass those headers through. Apply any embedded-browser header policy consistently to the document and all required assets.
   - The binary location sets a 30-day `immutable` policy for `.wasm` and related files. Use long immutable caching only for URLs proven to change when content changes; version changes and Cache Storage deletion cannot clear the browser HTTP cache or the CDN. Verify ArvanCloud response headers and cache status at the public hostname when diagnosing deployments.

---

## Coin Payment & Trade-In Architecture (تهاتر و پرداخت با سکه در فاکتورها)

GoldEx supports coin-based barter and payments (`PaymentType.Coin = 6`) across both Sell and Purchase invoices:

1. **Domain & Storage Integration**:
   - `InvoicePayment` aggregate holds `CoinInstanceId?`, `CoinQuantity?`, and `CoinUnitPrice?`.
   - Linked to `CoinInstance` with `Restrict` foreign key behavior.
2. **Double-Entry Accounting**:
   - In `AccountingTransactionService`:
     - Debits/Credits `SystemLedgerAccounts.CoinInventory` (کد معین کل موجودی سکه) vs Customer Accounts Receivable/Payable.
     - Supports cross-currency settlement with exchange rate conversion when paying in a currency different from the invoice currency.
3. **Inventory & Kardex Tracking**:
   - In `InventoryStockService`:
     - Sell Invoices (`PaymentSide.Receive`): Coin is received from customer $\rightarrow$ creates `WarehouseActionType.In` entry in stock and coin kardex.
     - Purchase Invoices (`PaymentSide.Pay`): Coin is handed to seller $\rightarrow$ creates `WarehouseActionType.Out` entry in stock and coin kardex.
4. **Friction-Free Instant Coin Entry**:
   - `InvoicePaymentService` automatically resolves or generates unique barcodes/creates `CoinInstance` on the fly when instant coins are registered, removing the friction of needing prior warehouse stock definitions.
5. **UI/UX & Live Price Synchronization**:
   - `PaymentEditor.razor` loads coins list, fetches live market price via `ICoinService.GetPriceAsync`, and binds `CoinUnitPrice` and `CoinQuantity` with auto-computed total amounts, while offering full manual override for price, quantity, mint type, package type, and workshop issuer.

---

## Real-Time Price Streaming & Visual Flash Indicator Architecture (پخش زنده قیمت‌ها با SignalR و انیمیشن درخشش کارت‌ها)

GoldEx uses an event-driven, real-time push architecture for market prices, replacing client-side timer polling with SignalR and hardware-accelerated visual indicators:

1. **Server-Side Change Detection & Efficient Dispatching**:
   - `PriceUpdaterBackgroundService` periodically triggers `IPriceUpdateOrchestrator.UpdateAllAsync()`.
   - In `PriceService.AddOrUpdateAsync`:
     - Compares incoming provider ticks against persisted `PriceHistory.CurrentValue`.
     - Detects genuine numeric price shifts (filtering out duplicate unchanged provider ticks to eliminate redundant network traffic).
     - Computes direction (`PriceChangeDirection.Up` or `PriceChangeDirection.Down`), formats values based on the default system `PriceUnit` (e.g. Rial to Toman conversion), and builds `PriceChangedNotificationDto`.
     - After persisting to the database via `repository.UpdateRangeAsync`, dispatches notifications through `IPriceNotificationPublisher`.
2. **SignalR Hub & Multi-Transport Infrastructure**:
   - `PriceHub : Hub<IPriceHubClient>` mapped at `ApiRoutes.Hubs.Prices` (`/hubs/prices`).
   - Strong typing via `IPriceHubClient.ReceivePriceUpdates(List<PriceChangedNotificationDto>)`.
   - Excluded from COEP isolation restrictions in `WebHostingExtensions` to permit seamless WebSockets handshakes.
   - `SignalRPriceNotificationPublisher` uses `IHubContext<PriceHub, IPriceHubClient>` to broadcast real-time ticks to all connected clients (`Clients.All`).
3. **Self-Healing & Resilient Client Architecture**:
   - `PriceStateService` maintains an in-memory thread-safe price cache and acts as the SignalR client listener.
   - **Automatic Reconnection**: Configured with `WithAutomaticReconnect([0s, 2s, 5s, 10s, 30s])` plus an exponential backoff jittered background loop (`RetryConnectionLoopAsync`) if the connection is permanently closed.
   - **Reconnection Healing**: On `Reconnected`, automatically calls `RefreshAsync()` to fetch a complete snapshot from the database, guaranteeing no missed price updates during temporary offline/reconnect windows.
   - **In-Memory O(1) Cache Patching**: Incoming price updates patch `List<GetPriceResponse>` in memory directly, updating all subscribed cards and calculators in 0ms without issuing secondary `GET /api/price` HTTP requests.
   - **Dormant Fallback Safety Timer**: A slow fallback polling timer (every 2 minutes) runs only when SignalR is disconnected (`HubConnectionState != Connected`), and is immediately stopped when the WebSocket connection is active.
4. **Visual Indicator (Green / Red Flash Animation)**:
   - `PriceCard.razor` and `MarketPriceDeck.razor` subscribe to `OnPriceChanged` and `OnPriceBatchChanged`.
   - When a price increase is received: activates `.price-card-flash-up` / `.market-ticker-card.flash-up` (soft emerald glow and border pulse).
   - When a price decrease is received: activates `.price-card-flash-down` / `.market-ticker-card.flash-down` (soft ruby glow and border pulse).

---

## Inventory Overview & Executive Stock Statistics Architecture (خلاصه وضعیت و موجودی کل انبار در پیش‌خوان)

GoldEx calculates store-wide inventory stock totals and weights directly at the database level using a dedicated high-performance aggregate endpoint:

1. **Database-Level Aggregation (`GetInventoryOverviewAsync`)**:
   - Instead of fetching paged records into client memory (which truncated results when stores exceeded 200 or 500 products), `IInventoryStockRepository.GetInventoryOverviewAsync` executes group aggregations in SQL via EF Core.
   - Converts mesghal to gram using the store's configured `GramPerMesghal` (e.g. 4.6083) for products and molten gold.
   - Accurately counts and sums total active stock (`CurrentQuantity > 0`) across:
     - **Manufactured Gold (`ProductType.Gold`, `ProductType.Jewelry`)**: Exact total weight in grams and total product count (e.g. 4,389 items).
     - **Molten Gold (`ProductType.MoltenGold`)**: Exact total weight in grams and count of molten gold pieces.
     - **Used Gold (`ProductType.UsedGold`)**: Exact total weight in grams and count of scrap/used items.
     - **Coins (`CoinInstance`)**: Exact count of coin instances in stock and total quantity.
     - **Currencies (`PriceUnit`)**: Distinct active currencies with formatted balance amounts, sorted descending.
2. **Unified DTO & Endpoint (`ApiRoutes.InventoryStocks.Overview`)**:
   - `GetInventoryOverviewResponse` encapsulates all stock metrics in a single lightweight payload (~200 bytes), replacing 5 separate heavy HTTP requests previously made by `RecentInventoryOverview.razor.cs`.
3. **Executive Presentation (`RecentInventoryOverview.razor` & `ExecutiveDashboard.razor`)**:
   - **Tab 3 («اجناس من»)**:
     - Top KPI cards display exact total weight and item count without any paging bias.
     - Stock composition section groups all physical inventory items together (Manufactured Gold, Molten Gold, Used Gold, and Coins) with distinct visual progress bars.
     - Average weight per manufactured item is derived from the complete database-wide inventory.
     - Currency section features distinct currency balance cards, active currency count pill badges, and an executive empty-state banner with direct 1-click action shortcuts when no currency balances exist.

---

## Executive Home Dashboard Architecture & Progressive Parallel Loading (پیش‌خوان اصلی و لود موازی و مستقل کارت‌ها)

GoldEx employs a high-performance, non-blocking dashboard architecture on the executive home view (`/?tab=0`):

1. **Problem Solved**:
   - Previously, `ExecutiveDashboard.razor.cs` executed sequential monolithic requests: fetching 500 full invoice entities with nested joins, calculating customer ledger balances across every customer and transaction in the system, querying category sales, and reading inventory overview all behind a single global `_isLoaded` flag.
   - As a result, users experienced multiple seconds of blank/skeleton screen time where all 4 KPI cards, trade charts, capital distribution donut, and unpaid invoice lists were blocked simultaneously until the slowest query finished.
2. **Dedicated Backend Aggregation Endpoints (`IDashboardService` & `IDashboardRepository`)**:
   - Registered under `ApiRoutes.Dashboard` (`/api/dashboard`):
     - `GetTodaySalesAsync`: Instant SQL aggregate filtering `InvoiceDate == Today && InvoiceType == Sell`, grouped by `PriceUnit` (returns amount and invoice count).
     - `GetCustomerBalancesSummaryAsync`: High-speed summary aggregating net customer receivables (our claims) and payables (our debts to customers) grouped by currency/unit, without transferring thousands of customer records over the network.
     - `GetTradeTrend30DaysAsync`: Scoped specifically to the last 30 days (`InvoiceDate >= Today - 29`), grouping invoices by date and type (`Sell` vs `Purchase`) and calculating exact 18K gold weight equivalents via the domain method `Invoice.CalculateTotalWeightEquivalent()`.
     - `GetTopUnpaidInvoicesAsync`: Evaluates recent open invoices, sorting by unpaid balance (`TotalUnpaidAmount`) and returning the top 5 records projected directly into `TopUnpaidInvoiceDto`.
3. **Progressive, Non-Blocking Parallel Client-Side Loading**:
   - In `ExecutiveDashboard.razor.cs`, the monolithic pipeline is replaced with independent parallel asynchronous tasks launched concurrently:
     - `LoadTodaySalesAsync()`
     - `LoadInventoryOverviewAsync()`
     - `LoadCustomerBalancesAsync()`
     - `LoadTradeTrendAsync()`
     - `LoadCategorySalesAsync()`
     - `LoadTopUnpaidInvoicesAsync()`
   - Each card and widget maintains its own state flag (`_isSalesLoaded`, `_isInventoryLoaded`, `_isCustomerBalancesLoaded`, `_isTrendLoaded`, `_isCategoryLoaded`, `_isUnpaidLoaded`).
   - In `ExecutiveDashboard.razor`, each KPI card and chart has dedicated skeleton placeholders. As each individual request resolves (often in under 100-200ms), that specific card smoothly transitions into its active data view and carousel without waiting for other components to finish.

### Customer Balance Sign Semantics

- Customer balances must be netted per customer and price unit across both the receivable and payable customer sub-ledgers using `Debit - Credit`.
- A positive net balance means the store has a receivable from the customer («طلب ما»); a negative net balance means the store owes the customer («بدهی ما») and must be displayed using its absolute value.
- Dashboard totals must classify and sum these per-customer net balances; they must not classify raw receivable/payable ledger columns independently, because one customer can have activity in both sub-ledgers.
- Invoice overview statistics must keep invoice direction explicit: a positive unpaid **sale** invoice is a store receivable, while a positive unpaid **purchase** invoice is a store payable. Purchase balances must not be included in a card labeled «مانده مطالبات».
- Negative invoice balances represent overpayments/credits and must be reported separately or netted at the appropriate customer-and-price-unit level; silently dropping them while summing only positive invoices can materially overstate outstanding receivables.
