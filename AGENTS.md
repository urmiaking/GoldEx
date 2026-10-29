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

- Local enterprise verification uses `GoldEx.Server.csproj` with launch profile `https` (`https://localhost:7013` and `http://localhost:5111`). After a user-authorized build, `dotnet run --no-build --no-restore --project src/App/Server/GoldEx.Server/GoldEx.Server.csproj --launch-profile https` runs the compiled host. Verify the fingerprinted appearance stylesheet linked by the returned page, as well as the unfingerprinted CSS URL, when checking whether theme edits are being served.

### Docker Build Versioning and Publishing

- Docker image versions are independent from the application release notes in `releases.json`. Do not increment or recreate the old tracked `.version` file.
- `scripts/docker-version.py` is the shared allocator used by `deploy.ps1`, GitHub Actions, and GitLab CI. Tags use `1.2.<UTC days since 2026-06-01>.<milliseconds within that UTC day>`, following the time-based approach of ITAM's Agent build script. The `1.2` family sorts above legacy `1.0` local/GitHub and `1.1` GitLab tags.
- Ignored `.docker-build-version` stores the last local allocation under a file lock. The allocator also reads the version label of each selected registry `latest` image and advances past the highest known build, even if the machine clock has moved backwards. Registry inspection errors must stop publication.
- Both Dockerfiles accept `APP_VERSION` and `APP_REVISION`, storing them as OCI image labels; these Docker build numbers must not be passed as assembly/file versions or replace user-facing release notes.
- Use the dedicated `ir.goldex.build.version` label for registry comparisons. Legacy images inherit `org.opencontainers.image.version = 24.04` from Ubuntu, so the generic OCI version alone must never be interpreted as a GoldEx version.
- Publish the numbered images first and check published versions again before promoting `latest`. GitHub concurrency and GitLab `resource_group` serialize their respective production jobs. This is not a distributed lock across local/CI machines; the accepted operating assumption is that those publish paths do not run simultaneously. See `docs/ai/DEPLOYMENT.md` for prerequisites and limits.
- After successful deployment, both CI pipelines validate and gracefully reload the running Docker Nginx container (`nginx` by default) using `nginx -t` followed by `nginx -s reload`. Deployment or validation failures must prevent reload; reload command failures must fail the job. GitHub uses tracked `scripts/deploy-affected.py` with the server's existing Compose file, while GitLab uses external `/masoud/initialize.sh`; deployment commands must return nonzero on failure and finish replacing the application containers before returning success. Override `NGINX_CONTAINER` through a GitHub Actions repository variable or a GitLab CI/CD variable when needed.

### GitHub Affected-Application Deployment

- GitHub's `.github/workflows/docker-image.yml` now uses `scripts/affected-apps.py` and the tracked `scripts/deploy-affected.py` instead of the external `refresh-apps.sh`. GitLab and local publishing remain independent. No enterprise release-note entry is needed for CI-only changes.
- Resolve both server entrypoints' transitive `ProjectReference` dependencies from Git snapshots. Folder names are insufficient: Calculator references enterprise Client.Components, Client.Services, Client.Abstractions, Shared, Application, Infrastructure, Domain and SDK projects. GoldEx.Client and GoldEx.Server themselves are enterprise-only. Check both old and new graphs, and both sides of renamed/deleted files. Unresolved graphs/history, dynamic references, explicit/implicit MSBuild imports and unknown runtime inputs rebuild conservatively; the XML scan must not pretend to evaluate MSBuild.
- Compare each application against its own last successful deployment commit, stored outside checkout at `$HOME/.local/state/goldex/github-deployments.json` (override `GOLDEX_DEPLOY_STATE_FILE`). Record only selected apps after Compose deployment AND Nginx validation/reload succeed. This recovers failed or superseded pending pushes; do not replace this with the last commit or push `before` SHA. Keep production workflow concurrency. Multiple runners targeting the same deployment must share this state path. Missing state initializes both apps; malformed state stops deployment.
- Build/version-check/promote only affected images. The deploy helper reads the existing production `/home/user/docker/goldex/docker-compose.yaml` (override `GOLDEX_COMPOSE_FILE`), maps all services using each selected image (including tenant instances), pins the generated immutable tag using a temporary Compose override, then runs `up --no-deps --no-build --pull never --wait` for those services only. Never use `--remove-orphans` or call the old all-app refresh script for selective runs. Preserve production mounts/environment and the original directory preparation under `/var/volumes/apps/goldex` (override `GOLDEX_APP_BASE`). Docker Compose V2 with `config --format json` and `up --wait` is required.
- Release events explicitly deploy both; manual workflows allow `auto`, `goldex`, `calculator` and `both`. Auto skips documentation/tests and unreferenced projects; fonts affect enterprise only; NuGet/build configuration and shared deployment tooling affect both. After deployments through GitLab/local/external tools, run manual `both` to reconcile GitHub's state before relying on auto selection.
- Verify with `python scripts/test_affected_apps.py`, `python scripts/test_deploy_affected.py` and `python scripts/test_docker_version.py`. These use temporary Git repositories/mocked Docker calls and do not publish or deploy applications.

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
- When reading or rewriting Persian source files through Python on Windows, always specify `encoding="utf-8-sig"` for reads (accepting existing BOMs) and `encoding="utf-8"` for writes. Never rely on `Path.read_text()`'s platform default: decoding UTF-8 through Windows-1252 corrupts Persian C# strings even when Razor text remains intact. The palette-name Unicode regression check is included in `scripts/tests/appearance.test.mjs`.
- Use MudBlazor components for application pages. The enterprise navigation shell, native calculator tabs and responsive customer editor dialog chrome are explicit user-requested exceptions: the user requested a complete native Blazor AppBar/NavMenu migration matching MeetFlow and Mkx, using `ShellIcon` line SVGs. Keep the calculator tool forms and the rest of the application on MudBlazor.
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
- Keep profit-analysis reporting isolated from invoice posting: do not alter AccountingTransactionService.BuildTransactionsForInvoiceAsync or its purchase/COGS lookup solely for report logic.

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

- **Server startup dependency**: `GoldEx.Calculator.Server` still requires `ConnectionStrings:GoldEx` for the shared `GoldExDbContext` and SQL-backed data-protection keys, and its startup checks/applies that context's migrations. The offline-first behavior describes client invoice/profile storage, not a database-free server host. For local runs, supply the existing development database configuration through the calculator process environment when its own configuration does not contain the connection string; never record the value in documentation.

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

1. **Current Desktop Navigation**:
   - The user explicitly replaced the old GoldEx MudAppBar/Mini Drawer/MudNavMenu with the native workspace shell from MeetFlow's **active `Layout/MainLayout.razor`** and Mkx.Templates.Blazor. MeetFlow's separate `Layout/Components/AppBar.razor` and `NavMenu.razor` are legacy files, not the active reference.
   - GoldEx `Layouts/MainLayout.razor` renders a permanent 238px RTL sidebar from 960px, a fixed native header, native `NavLink` menu and native mobile bottom navigation. `Drawer.razor` now owns the native `<aside>` and mobile modal; it no longer renders MudDrawer. No legacy desktop top menu or Mini Drawer remains in the active enterprise shell.
   - `ShellIcon.razor` uses the same 24px/1.65-stroke SVG family as MeetFlow, with GoldEx-specific calculator, inventory, store and finance glyphs. Preserve shell-wide icon consistency rather than returning to filled Material icons.
   - Store selection, invoice creation and account/sign-out live in the sidebar. The six color choices were explicitly removed from NavMenu and moved to the globally available left-edge `PalettePicker`. The header retains date/clock, mode switching, contextual help, unread notifications and the account avatar. The header profile avatar links directly to ClientRoutes.UserAccounts.Index (`/user-accounts`) at the user's request. The old profile popover and its code-behind are removed. Appearance controls, license status, health and administrator logs access live in a lazily-rendered «ظاهر و وضعیت برنامه» expansion on that account page; logout remains in the sidebar. All previous authenticated navigation destinations, public calculator/about links and custom-domain vitrine links are preserved.
2. **Lazy-Loaded View Rendering**:
   - `Pages/Home/Index.razor` conditionally renders the active panel: market/executive dashboard, calculator, `RecentInvoicesOverview`, `RecentInventoryOverview`, or `CustomersInsightOverview` via `@if (_activePanelIndex == N)`.
   - Preserve conditional rendering during visual redesign; performance improvements require measurement rather than assuming a fixed DOM reduction.
3. **Custom Segmented Tab Controller & Smooth CSS Entrance Animations**:
   - Replaces default heavy MudBlazor tabs with `.goldex-segmented-container` pill tabs.
   - CSS entrance animations (`.animate-fade-in-up`) accompany panel changes; actual frame rate depends on the target device.

---

## Mobile Navigation and Notification Layering

- Below 960px, the native `.gex-sidebar` becomes a modal drawer, up to 328px/88vw wide. Its scrim covers the entire viewport (including header and bottom nav), at `--mud-zindex-drawer`; the sidebar is one level above it. Only the menu list scrolls; brand, create button and identity remain visible.
- The shared RCL module `_content/GoldEx.Client.Components/js/navigation.js`, adapted from Mkx, locks scrolling only while the mobile menu is open, closes any open floating palette and makes workspace/bottom nav/floating palette inert, wraps keyboard focus and restores original inert values on close/disposal or desktop breakpoint changes. Escape, scrim clicks and route navigation close the menu. Close/release the menu before opening the store selector dialog.
- Native `.gex-bottom-nav` sits at `--mud-zindex-drawer - 4`, below the header (`-3`), overlays and notifications. Signed-in mobile destinations are Home, Calculator, central Quick Create, Customers and Invoices; inventory remains in the sidebar. Guests see Home, Calculator, Login and About.
- The mobile center plus now opens `QuickCreateMenu.razor`, a native HTML `<dialog>` action sheet (never MudDialog for the launcher), via RCL `js/quick-create.js`. It has New Invoice, New Customer, Manual Inventory Entry, Customer Transfer and Payment Voucher actions. Browser-native modality handles focus containment/background inertness; the module closes palettes, locks scrolling while open, and closes on backdrop, Escape, action selection, route change, desktop resize and disposal. The existing destination forms remain unchanged UI workflows (their pre-existing MudDialog editors are reused where applicable).
- Quick Create is offered only to `Administrators`/`Owners`, matching all five destination pages; other signed-in users get account access in the center. Customer, transfer and payment list pages consume a one-shot `?create=true` command after render, replace the URL with that parameter removed, then wait for the clean-URL render before calling their existing list `OnCreate` methods. MudDialogProvider dismisses existing editors on navigation, so removing the query and opening the editor in the same render races and immediately closes it; preserve this two-stage handoff. Transfer creation waits for its shared initialization task to load price units before opening its editor; do not race an empty units list or change financial posting logic.
- `ShellBrand.razor` displays **GoldEx**, using the original global `.brand-text` and locally bundled **Cinzel Decorative** font/gold treatment in both sidebar and mobile AppBar. At the user's request, its mark is the original transparent GoldEx app icon (`/apple-touch-icon-180x180.png`), shared with the app's touch icon, rather than the generic gem SVG. Keep both icon and wordmark visible below 430px with a smaller icon and gap; do not hide either on small phones.
- `Notifications` is mounted at shell root and uses a browser-native `<dialog>` styled as MeetFlow's **left-edge full-height drawer** (420px from 960px; full viewport width below 960px, square edges and a light scrim), with a single chronological list (no archive tab), a summary, read/dismiss actions and device Push controls. The shared RCL `js/notifications.js` owns modal focus/scroll cleanup, backdrop/Escape dismissal, subscription registration, VAPID-key replacement and persistent device opt-out (`GoldEx.PushDisabled`). MainLayout and AppBar expose notifications only to Administrators/Owners, matching the API. Public vitrine and standalone Calculator remain separate.
- Notification read/dismiss state is per user in `NotificationUserStates`; legacy `Notification.IsRead` remains a compatibility baseline. Dismissing does not delete the underlying invoice reminder, preventing regeneration. Lists return the most recent 100 visible records and refresh every minute/on opening. SMS actions remain explicit user actions; do not automatically send reminder SMS when dispatching Push.
- Web Push is documented in [NOTIFICATIONS.md](./docs/ai/NOTIFICATIONS.md). The worker iterates active stores using a fresh scope and explicit StoreContext, uses the existing invoice balance formula with payments loaded, and only creates reminders for positive unpaid balances after the due date. Serializable generation prevents concurrent duplicate creation; a unique per-device delivery index and atomic two-minute leases protect dispatch. Pending sends expire after a day, retry temporary failures up to five attempts, recheck store membership/roles and invoice status, and disable 404/410 endpoints. Generic Push text excludes customer names and balances.
- `NotificationPushRepository` explicitly scopes queries to the HTTP request's resolved IStoreContext (falling back to worker DbContext.CurrentStoreId). GoldExComponentBase creates child service scopes during SSR, whose fresh StoreContext can otherwise incorrectly fall back to the default store. Any IgnoreQueryFilters in this repository must retain an explicit current-store or current-user condition. Its browser-endpoint lookup is the deliberate cross-store exception: one endpoint hash is globally unique so signing in or switching stores replaces the old browser binding and cancels its old queue; never return another user's subscription to the client.
- The user explicitly requested a newly-generated GoldEx VAPID pair in the main server `appsettings.json`, under `Notifications:Push`. Only Enabled/PublicKey are exposed through IPushNotificationService; never send PrivateKey to clients/logs or rotate the key on startup. Both development and published enterprise service workers import the same root `notification-worker.js`; preserve their existing PWA/cache rules. The browser requests permission only from the explicit enable action; granted subscriptions restore on sign-in, while device opt-out is respected. Logout revokes persisted subscriptions for that user across stores. Provider URLs/keys are validated; outbound redirects are disabled and sends have a 20-second timeout.

## UI Design Review Reference

- The initial code-based comparison with the sibling `Mkx.Templates.Blazor` template is recorded in [UI_DESIGN_REVIEW.md](./docs/ai/UI_DESIGN_REVIEW.md). The implemented palette, mode transition and AppBar phase is documented in [APPEARANCE.md](./docs/ai/APPEARANCE.md); the remaining review proposals are not implementation decisions.
- GoldEx already has six paired light/dark palettes in `GoldEx.Client.Components/Themes/ColorPalettes.cs` and persisted appearance in `ThemeService`; improve these existing mechanisms before adding another theme system.
- Mkx and MeetFlow use native Blazor navigation. The user explicitly requested copying that shell into GoldEx; this supersedes the earlier appearance-review recommendation to retain MudBlazor navigation. Application pages and render modes remain unchanged.
- Shared presentation rules in `GoldEx.Server/wwwroot/css/app.css` affect nested MudBlazor surfaces and animations. Inspect both these rules and component-local styles before changing card borders, shadows, motion or dialog scrolling. The current `.goldex-glass-card` has an opaque theme surface despite its name.
- `AppearanceControls` in AppBar/account management and the floating `PalettePicker` share `ThemeService.IsChanging` and the asynchronous `OnAppearanceChanged` event. BaseLayout acknowledges its completed render before the appearance module captures the new MudBlazor stylesheet; preserve this acknowledgement and release event subscriptions on disposal.
- The appearance module is served from the shared Razor library at `_content/GoldEx.Client.Components/js/appearance.js`. Calculator imports the same BaseLayout and theme contract, so do not move that module into enterprise-only server assets or remove its legacy `ToggleMode()` entry point without updating Calculator.
- The user requested Mkx's floating palette: `PalettePicker.razor` is mounted once at enterprise MainLayout root, outside page animations. Its shared RCL `js/palette-picker.js` is adapted from Mkx, uses native popover with a connected SVG sheet, supports mouse/pen/touch drag plus ArrowUp/ArrowDown/Home/End, clamps between header and bottom nav (including visual viewport changes), and stores a normalized position under `GoldEx.PickerPosition`. Palette choices use the existing six `ColorPalettes` and `IThemeService.SetPaletteAsync`, preserving mode/persistence/reveal behavior. Remove sidebar swatches entirely; do not reintroduce `NavigationAppearance`. The floating tab stays available on mobile; it has no hide action requiring recovery through NavMenu. Route changes close the sheet; disposal removes all event listeners. Public vitrine and standalone Calculator are unaffected.
- Enterprise geometry in `appearance.css` now explicitly matches MeetFlow at the user's request: AppBar is **82px from 960px**, **72px above 650px and below 960px**, and **70px through 650px**, plus top safe area. Content retains its 12px gap. This latest request supersedes the earlier compact 60px/56px header preference. `--gex-content-offset` and `--gex-table-layout-delta` automatically shrink all five responsive-table variants by the additional header height (22px desktop, 16px tablet, 14px phone); keep the original 56px table baseline, and do not subtract the increase a second time in individual table rules. The sidebar spans the full viewport. Keep the default table delta zero outside the enterprise shell and do not globally hide body overflow.
- Mobile main-content horizontal padding is restored to **0 below 960px** (desktop stays 16px). Commit `3422185c` added 12px per mobile side, reducing usable body width by 24px; the user explicitly requested the earlier width. Before that commit, `MainLayout` used `py-4` on mobile and `pa-md-4` only from 960px, so mobile shell padding was zero. Preserve page-level gutters rather than adding a second shell gutter.
- Navigation and palette regression checks: `node --test scripts/tests/navigation.test.mjs scripts/tests/palette-picker.test.mjs scripts/tests/quick-create.test.mjs scripts/tests/appearance.test.mjs` covers mobile focus wrapping, inert-state preservation, breakpoint release/disposal and the existing appearance transition contract. Local verification must also exercise actual signed-in sidebar, notifications and store selection.
- Preserve the original `CreateGoldTheme` identity and base theme, including vivid Info/Success/Error colors, contrast text, secondary text, default neutral table/input lines and 24px radius. The latest explicit user request permits the light surface/background change documented below. The user rejected both the initial darker gold redesign and the partial restoration: darkened blue/green/red actions, dark gold-button text and gold table lines still looked less lively. The Gold factory and base theme were first restored verbatim from the original revision; the subsequent requested card separation is the only light surface/background exception. Keep the redesigned non-Gold palettes, appearance animations and the current native AppBar. Do not reinterpret Gold's colors again without a new explicit user request, or globally mute semantic AppBar icon colors.
- The user rejected the first subtle Gold shadow polish as imperceptible and explicitly requested Ocean-like card definition. Gold now uses the same shared card and hover shadows as Ocean from `app.css`; do not reintroduce weaker Gold-only shadows. In `CreateGoldTheme`, light `Surface` is white (`#FFFFFF`) and `Background` is warm `#F5F3EE`, separating cards from the page like Ocean while preserving Gold accents and semantic/text colors. These shared palette values also apply to Calculator. The remaining enterprise-only primary-button highlight and native workspace styling live in `appearance.css`, scoped to `html[data-gex-palette="Gold"] .goldex-shell`; Calculator and public vitrine do not load this stylesheet. The existing appearance module supplies the root palette/mode attributes after browser synchronization.

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

---

## Customer Overview Statistics (Home Tab 4)

- `CustomersInsightOverview` must get total and customer-type counts from `ICustomerService.GetOverviewAsync` (`GET /api/customers/overview`), which aggregates all customers under the active store's global query filter. Never derive these statistics from the paged recent-customer list.
- The recent-customer request fetches only five records. Retail counts include `RetailCustomer` and `Retailer`; the colleague group includes `Wholesaler`, `MeltedGoldDealer`, `Workshop`, and `AssayingLab`, covering every current customer type.
- Customer percentages use decimal arithmetic and invariant formatting so fractional percentages remain valid CSS widths. Refresh both the overview and recent list after creating a customer from the home tab.

---

## Profit Reporting Data Semantics

- `ReportingMapper` maps `SellInvoiceRpResponse.TotalProfit` from `Invoice.TotalProfitAmount`, which sums invoice item profit amounts. This is the invoice pricing profit component, not a sale-minus-acquisition-cost margin or a gold holding gain calculation.
- `AccountingTransactionService` calculates product COGS using a matched invoice item's base-currency amount per unit of weight multiplied by the sold weight. Its lookup is restricted to purchase invoices containing the product, no later than the sale (using creation time for same-day documents). Existing ledger entries are not rewritten by the profit report.
- `Price.PriceHistory` is an owned current-price object updated in place by `Price.SetPriceHistory` and `PriceService`; it must not be treated as a time series for historical inventory valuation.
- Historical invoice reporting should use stored item amounts and transaction-time rates. `InvoiceProductItem.RecalculateAmounts` reads some current product attributes, so recomputing old invoices from today's product data is not a safe substitute for historical snapshots.
- `/reporting/profit-analysis` and `IReportingService.GetProfitAnalysisAsync` implement the first phase: realized sales analysis for manufactured gold and jewelry. Access is restricted to Administrators/Owners at both the page and API. `ProfitAnalysisOverview` shows the current culture's month-to-date summary on the home dashboard with an independent service scope.
- The report reconstructs acquisition attribution from stored invoice lines; it does not add a persisted lot-allocation table or post accounting entries. It requires exactly one preceding purchase line per product and checks all preceding sold weight, including sales outside the selected dates/customer. Ambiguous origins, missing historical data, manual entries, non-invoice stock movements and reversals are explicitly excluded from totals with a reason.
- Net line revenue excludes tax and includes proportional invoice discounts; purchase cost includes its proportional purchase discount. Additional invoice charges and shop operating expenses are outside this report. Gold weight equivalent comes from stored sale raw amount divided by stored sale gram price, not current product weight/purity.
- Currency decomposition uses transaction-time exchange rates and groups totals by the sale document's base price unit. Gold-rate movement is translated at sale FX; acquisition-cost FX movement is shown separately. Trading margin + gold effect + FX effect equals gross profit. IRR/TMN conversion is exact; incompatible currencies or missing FX are never converted at live rates or silently treated as 1.
- Unrealized inventory valuation, historical market-price snapshots, pooled inventory costing, and molten/used gold, coins and currency trades are outside phase one. The page states these limits. Data retrieval keeps store query filters and projects only selected products' document history. The report returns every calculated row in one response, without `Page`/`PageSize`; `SummaryOnly` still omits items for the dashboard. Like `SellInvoiceReportView`, the screen uses `MudTable Virtualize="true"` over the in-memory list, not server paging or an `ItemsProvider`. Print uses the same complete list.
- Blazor `[SupplyParameterFromQuery]` must not target enum/nullable-enum properties: the query value supplier throws during rendering even if the parameter is absent. Bind enum filters to strings and parse/validate them in `ReadQueryToFilter`, as in `SellInvoicePageBase` and `ProfitAnalysis`.
- Weight-denominated invoice lines (Gold18K/Mesghal) can participate when their document has a positive stored exchange rate to a monetary base. Both the saved gram price and line cost are converted using that document's historical rate; missing rates remain incomplete. For these lines, gold-rate movement is compared directly in the common monetary base and FX is not double-counted. This is historical inventory/sales margin analysis; revaluation of outstanding weight-denominated payables is outside the report.
- Reporting UI follows the existing `Pages/Reports` + `Pages/Base` + `Components/Filters`, `Views`, `Summaries` + `Pages/Prints` separation. Keep report pages thin and use `ReportFiltersBase`, the standard toolbar/table, `ToCurrencyFormat`/`ToWeightFormat`, and summary cards after the table. Profit analysis uses theme primary text for monetary values; avoid light success/warning text on light surfaces. Per user preference, incomplete/instant-sale rows are omitted from the returned items and from screen/print, with no «نیاز به بررسی» statuses or counts in the UI. Response candidate/excluded counts remain diagnostic metadata; displayed row counts use `CalculatedCount`. Exclusions must never turn into zero-cost/profit rows or alter valid totals. Expandable details show the related purchase for calculated rows.
- The reporting index uses the short title «تحلیل سود فروش» so its existing `MudActionCard` aligns with neighboring cards. The dashboard profit summary belongs below the established KPI/chart sections and reuses `goldex-glass-card` and section heading patterns; it is not an extra banner above the main KPIs. Its link carries the displayed period. Profit print uses the common `ReportPrintBase` and the complete report response, independently of which virtualized rows are currently rendered on screen.
- Report action styling follows `SellInvoiceReportView` and `PurchaseInvoiceReportView`: print uses `Color.Success` with `Variant.Filled`, and invoice-view actions use small `Color.Info` filled icon buttons with matching tooltips. Do not override button/SVG colors with report-wide text-color CSS. Profit analysis uses complementary `SmAndDown` / `MdAndUp` visibility rules so the desktop print button and mobile print icon cover every viewport width.
- Profit analysis dates default to empty (`DateRangeExtensions.From(Start, End)`), and the page loads all available history when no dates are supplied. The API, repository and builder must treat a missing start/end as an unbounded side, without substituting today/current month or enforcing a one-year cap. Explicit date filters remain inclusive; preceding history is still required for cost allocation. Clearing a dated URL must reload the unbounded report, including for print. The dashboard's month summary continues to request its explicit month-to-date period. Avoid `AddDays(1)` overflow for a maximum or absent end date when filtering stock movements.

---

## Existing Backup Service Contract

- Backup and restore already have a shared contract in `src/App/Shared/GoldEx.Shared/Services/Abstractions/IBackupService.cs`, exposing `GetBackupFilePathAsync` and `RestoreDatabaseAsync`, and a settings UI in `src/App/Client/GoldEx.Client/Pages/Settings/BackupRestore.razor`.
- When planning backup improvements, inspect the existing implementation first; the shared contract alone does not establish scheduling, retention, or restore verification behavior.

---

## Smart Showcase Tray Architecture (سینی هوشمند پیش‌خوان)

GoldEx provides a real-time smart tray system designed to track items removed from jewelry showcases during customer viewings and counter negotiations:

1. **Domain Aggregate & Multi-Tenancy**:
   - `SmartTray` aggregate root (`GoldEx.Server.Domain/SmartTrayAggregate`) and child entity `SmartTrayItem`.
   - Both entities implement `IStoreFiltered` with explicit `StoreId` properties to prevent EF Core global filter mismatch warnings.
   - Indices on `[StoreId, TrayNumber]` (non-unique index with auto-incrementing sequential numbering to prevent duplicate crashes) and `[StoreId, Barcode]` guarantee sub-millisecond barcode lookups.
   - States: `SmartTrayStatus` (`Active`, `Closed`, `HasDiscrepancy`) and `SmartTrayItemStatus` (`OnTray`, `Sold`, `Returned`, `Missing`).

2. **Dual-Mode Barcode Scanning**:
   - **Desktop HID Wedge Listener**: `smart-tray-scanner.js` detects barcode scanner keystrokes (velocity < 50ms), normalizes Persian numerals (`۰-۹` to `0-9`), filters out manual typing inside inputs, and calls Blazor via `[JSInvokable]`.
   - **Mobile Camera Continuous Scanner**: `ContinuousBarcodeScanner.razor` uses ZXingBlazor with `Decodeonce = false`, frame error suppression, 1.5s debounce, camera switching, and persistent DOM preservation via Blazor `@key`.
   - **Manual Search & Input**: Direct text entry field with instant autofocus.

3. **Zero-Latency Audio & Haptic Feedback**:
   - `smart-tray-audio.js` synthesizes sounds in real-time via the HTML5 Web Audio API (no audio file downloads or lag):
     - Scan success: Double chime (C6 $\to$ E6) + short vibration.
     - Return success: Mellow chord (A5 $\to$ E5) + pulse vibration.
     - Warning: Low sawtooth alert tone.
     - Discrepancy alarm: Pulsing two-tone siren.
     - Tray completed: Uplifting major arpeggio.

4. **Safety & Missing Item Audit Flow**:
   - Salesperson selects between mode 1 (خروج به پیش‌خوان) and mode 2 (برگشت به ویترین).
   - Real-time dwell stopwatch (`DurationSeconds`) tracks how long each jewelry item has remained on the tray.
   - Closing a tray while items are still present launches `SmartTrayDiscrepancyDialog.razor`, requiring discrepancy reason logging and sounding local audio/visual alarms (no external SMS).

5. **Multi-Item Invoice Transfer**:
   - Salesperson checks desired items and clicks «انتقال به فاکتور فروش».
   - Navigates to `/invoices/new/sell?barcodes=BC1,BC2,...`.
   - `SetInvoice.razor` and `EditorForm.razor.cs` automatically parse and append all selected items into the active invoice grid.

6. **Real-Time Store Sync**:
   - `SmartTrayHub` (`/hubs/smart-tray`) streams real-time state changes (`ItemAdded`, `ItemReturned`, `ItemTransferredToInvoice`, `DiscrepancyLogged`) across all active devices in the store via SignalR.

---

## Page & Element Smooth Motion Choreography Architecture (انیمیشن نرم جابجایی بین صفحات و عناصر)

GoldEx implements a unified, high-performance page and element transition choreography system inspired by Apple interface design standards and MeetFlow:

1. **Jitter-Free Stable Scroller**:
   - `html { scrollbar-gutter: stable; overflow-x: clip; } body { overflow-x: clip; }` prevents horizontal scrollbar layout shifts (jumps) when navigating between short and tall pages.
2. **Motion Tokens & Natural Curves**:
   - `--gex-motion-ease: cubic-bezier(0.22, 1, 0.36, 1)` provides an organic, decelerated ease-out curve.
   - Core keyframes: `gex-page-reveal` (subtle vertical rise + fade), `gex-section-reveal` (staggered cards & panels), `gex-soft-pop` (badges, chips, empty states), and `gex-dialog-pop` (modals).
3. **Route Navigation Transitions**:
   - `MainLayout.razor` wraps `@Body` in `<div class="goldex-page-container page-transition">`.
   - `SettingsLayout.razor` wraps `@Body` in `<div class="goldex-settings-content page-transition">`.
   - `VitrineLayout.razor` wraps `@Body` in `<div class="vitrine-page-content page-transition">`.
   - `EmptyBoxLayout.razor` applies `.page-transition`.
   - `GoldEx.Calculator.Client` layout (`MainLayout.razor`) wraps `@Body` in `<div class="px-2 px-md-4 calc-page-container page-transition" @key="Navigation.Uri">` with `IDisposable` subscribing to `Navigation.LocationChanged` to auto-close drawers.
   - Dual CSS synchronization: `calculator-luxury.css` is maintained identically in both `src/Calculator/Client/GoldEx.Calculator.Client/wwwroot/assets/css/` and `src/Calculator/Server/GoldEx.Calculator.Server/wwwroot/css/`.
4. **Staggered Element-Level Choreography**:
   - Page headings, breadcrumbs, search bars, and toolbars animate first (`gex-section-reveal` / `calcSectionReveal`, 0.32s).
   - Primary cards, tables, data grids, and dashboards cascade with staggered delays (`:nth-of-type(2)`: 45ms, `:nth-of-type(3)`: 85ms, `:nth-of-type(n+4)`: 125ms).
   - Grid items animate with incremental stagger (`35ms`, `70ms`, `105ms`, `140ms`).
   - Calculator-specific elements: `.calc-segmented-nav` with smooth active indicator glow, `.coin-card` with hover elevation and staggered entrances, `.calc-breakdown-bar` animated segment widths, and quick invoice floating FAB (`quick-invoice-fab.css`) with `calcFabSpring` and speed-dial pop.
5. **Tactile Micro-Interactions**:
   - Action cards, quick-action pills, price boards, and inventory items feature hardware-accelerated hover lift (`translateY(-2px)`) and smooth shadow elevation.
   - Buttons respond with tactile press feedback (`:active { transform: translateY(1px) scale(0.98); }`).
   - Mobile navigation bar tab icons animate with smooth lift and scale (`translateY(-2px) scale(1.08)`).
6. **Modern View Transitions API & Accessibility**:
   - `@view-transition { navigation: auto; }` enables seamless cross-document view transitions in modern browsers (Chromium 126+, Safari 18+).
   - Full `@media (prefers-reduced-motion: reduce)` compliance disables all transitions for users with motion sensitivity.



## Notification verification follow-up (2026-10-27)

The user explicitly confirmed receiving a real device notification after enabling permission («حله نوتیف گرفتم»). Keep this human confirmation separate from mock-provider transport tests. SQL tests use an isolated temporary LocalDB database for lease, membership, tenant and overdue-balance checks; they never modify financial records in the application's development database. Device Push initialization failures must preserve the in-app notification list and provide permission/network feedback.

- Notification drawer geometry supersedes the initial centered Mkx panel: keep physical left anchoring (`inset: 0 auto 0 0`, zero margin), 100dvh height, safe-area header/footer padding and internal list scrolling. Native dialog modality continues to own focus containment and Escape/backdrop cleanup. Do not restore centered positioning or phone gutters around the drawer.


## Calculator workspace tabs (2026-10-27)

- Enterprise `/calculator` delegates its tabbed workspace to `Pages/Calculate/Components/Calculator.razor`. Its six tool titles/descriptions/line icons live in the private Tools metadata in Calculator.razor.cs: simple calculator, Smart Tray, reverse calculation, currency conversion, gold parity and advanced search. The `?tab=smart-tray`/`tray` entry and HelpContext remain intact. The first calculator is public; VisibleTools filters the other five using the authenticated GoldExComponentBase.User state. Selection also rejects authenticated-only tool indexes for guests.
- The user clarified that calculator tabs must be **native HTML/Blazor, with no MudTabs or MudTabPanel**, because MudBlazor's overflow arrows move unreliably in RTL. Calculator renders button/tabpanel pairs with unique IDs, aria-controls/aria-labelledby, aria-selected and roving tabindex. Only the active tool component is instantiated; inactive panel containers stay hidden. The connected workspace styling remains scoped to `.gex-calculator-*`.
- Shared RCL `js/calculator-tabs.js` owns only horizontal scrolling and keyboard/focus behavior. The left/right buttons mean physical screen directions; use native scrollBy with negative/positive left deltas and bounding rectangles for edge states, never infer physical direction from an RTL scrollLeft convention or transform the tab track. Arrows never select another tool. Arrow keys follow RTL geometry and move focus; Enter/Space activate the native buttons, Home/End jump to boundaries. Selected tabs are revealed without scrolling the document vertically. Resize/Mutation observers and listeners must be disposed. Arrow controls are hidden when no overflow exists, and reduced motion uses instant scrolling. Regression checks are in scripts/tests/calculator-tabs.test.mjs.
- Home uses Calculator with HideTabs=true, rendering SimpleCalculator directly. Preserve that branch and its Class/Elevation parameters; the redesigned tab frame must not appear on Home or in the standalone Calculator app. The native correction was compiled and run as follow-through to the user's explicit compilation request for this calculator redesign.

- ASP.NET Core MVC validates positional record DTOs using constructor-parameter metadata. Push request DataAnnotations must target the primary constructor parameters (default attribute target), never `[property: ...]`; the latter throws during browser subscription registration. NotificationPushModelValidationTester exercises the real MVC object validator to guard this integration behavior.

- Some Chromium browser subscriptions use the official legacy Google endpoint `https://jmt17.google.com/fcm/send/`. Accept that exact HTTPS host/path in PushSubscriptionValidator alongside FCM; do not broadly allow google.com or rewrite the browser's endpoint to another provider host. The live refresh regression was caused by rejecting this host during granted-subscription restoration. Validation failures now carry named field errors; never throw message-only FluentValidation exceptions without preserving their message in the response (the exception middleware supplies a fallback error when Errors is empty). Diagnostic warnings include error codes/provider hostname only, never endpoint tokens or subscription keys.

- The notification drawer is explicitly full-width throughout the mobile shell breakpoint (below 960px): `width: 100vw` and no outer side border. Desktop retains its 420px width. Preserve full-height positioning and existing focus/scroll/Push behavior.

- Calculator tab text/icons are explicitly neutral for inactive tools (`--mud-palette-text-primary`: near-black in light mode, readable neutral text in dark mode). Only `.is-active` uses the palette primary accent/indicator. Inactive hover must also stay neutral; do not color every tab with the Gold secondary text color.


## Customer directory cards (2026-10-27)

- Enterprise `/customers` now uses a responsive card directory in `Pages/Customers/Components/CustomersList`, replacing MudTable rather than restyling its mobile rows. `CustomerCard.razor` is a display-only component with stable initial avatars, customer type/code/phone, balance and callbacks for the existing editors, removal confirmation, transaction list and invoice list. Scoped CSS uses current palette tokens and one/two/three columns; there is no fixed table-height container or nested list scroll.
- CustomersList now loads 30 customers initially and appends the next 30 through Show More, replacing numbered pagination. It retains the same five sort keys, debounced name/code/phone search, and CustomerFiltersDialog (customer type, debit/credit, registration dates). Search/sort/filter changes restart at 30; successful editor refreshes reload the already-revealed window. A request version guard plus GoldExComponentBase cancellation prevents older searches overwriting newer results. Keep API calls through GoldExComponentBase and preserve the public OnCreate hook used by the two-stage `?create=true` flow in Customers/Index.
- Card balances still load asynchronously through the shared CustomerRemaining component. Its opt-in ShowStatus adds the existing sign interpretation as text (positive = بدهکار, negative = بستانکار, zero = تسویه); default display and monetary calculations remain unchanged for all other callers. Do not derive customer balances from this paged directory query or replace its multi-unit carousel with a single fabricated total.

- The global app stylesheet already styles `.customer-avatar` with an important secondary text color for invoice widgets. Directory initial avatars use the distinct `.directory-avatar` class to avoid that collision. Neutral card surfaces use `--mud-palette-lines-default` and `color-mix` with text-primary; do not assume MudBlazor emits `--mud-palette-action-default-rgb` (it is absent in the live Gold palette).

- The user requested a more compact directory card after the first preview. Cards now use 12px padding, a 40px initial avatar, a single three-column contact/code/registration row, a horizontal balance strip and 44px action targets. Keep registration dates inline instead of restoring the separate bottom date row.

- Customer card actions use explicit `customer-action-ledger` and `customer-action-invoices` classes: bordered tonal buttons with inset icon backgrounds, palette-primary for ledger and palette-info for invoices. Keep neutral readable labels, 44px touch targets and the compact card height; both actions still invoke their existing callbacks. Avoid restoring the previous bare text-button appearance.


## Customer directory sticky filters and progressive loading (2026-10-27)

- The directory heading and New Customer button scroll away normally. `.directory-filters` then sticks below the native AppBar using `--gex-appbar-height` plus the top safe area and 8px; search, sort and active filter chips remain available while the cards scroll in the document. Keep this local to CustomersList and preserve shell/header/bottom-nav stacking; do not globally lock body scrolling or introduce a second fixed-height card scroller.
- Show More requests use a 30-record batch, raw consumed offset and the existing total. Appends keep old card instances/balances alive; repeated pending clicks are guarded. Query changes cancel earlier work, discard stale responses and restart the list. Append failures preserve existing cards/offset and offer retry. IDs are deduplicated for concurrent edits without basing the next server offset on the deduplicated count.
- The active customer repository uses QueryableExtensions.ApplySorting (not CustomersByFilterSpecification) for its directory query. It now adds NationalId and CustomerType as deterministic tie-breakers before Skip/Take; that combination is unique within a store. Keep this ordering and tenant scoping when changing progressive loading. CustomerOverviewTester covers tied names/dates/phones, 30/30/6 batches and tenant exclusion.
- CustomerCardSkeleton mirrors the compact card for initial and append loading. Keyed CustomerCard instances animate once on insertion with a capped stagger and respect reduced motion; existing cards must not replay their entrance when more records are appended. The first newly appended card receives focus without scrolling for keyboard continuity. Filters and footer announcements remain accessible.

- Live progressive-directory verification loaded 30 then 60 distinct customers, preserved all 30 existing menu/component identities and the mobile scroll position, restarted at 30 after clearing search, and hid Show More for a one-customer result. Sticky filter tops were 78px under a 70px mobile header and 90px under an 82px desktop header. The six CustomerOverviewTester cases passed.

- Customer filters below 960px now occupy a single sticky row: search plus a 44px icon button. Direct sort controls and applied-filter chips are hidden at that breakpoint; their state is exposed by an accessible settings count on the button. CustomerFiltersDialog receives/returns the sort key and direction together with its date/type/account filters; apply resets progressive results to the first 30 in one request, cancel preserves all current settings, and Reset All restores registration-date descending. Desktop retains direct sort controls synchronized with this same state.

- The direct customer sort-direction tooltip is enclosed by a native `.directory-sort-toggle` wrapper, which is hidden together with the sort field below 960px. MudTooltip Class does not decorate its layout root in the current package, so hiding a class passed to MudTooltip leaves a stray arrow/grid row; hide the native wrapper instead.

- The single-row customer toolbar was verified at 320px and 414px with two visible controls and about 70px height, including combined active account/sort settings. Cancel, apply, retained dialog selections and Reset All were checked live; reset restored 30 customers and no badge. Desktop retains four visible controls and registration-date descending, and the mobile sticky top remains 78px below the 70px header.

- Customer directory controls must inherit MudBlazor's theme border radius (`--mud-default-borderradius`, 24px in Gold and 16px in the other existing palettes). Do not add local 10/11/12px radii to search inputs, filter/create/load-more buttons or card action buttons. The previous search wrapper radius conflicted with its theme-rounded outline fieldset; remove that override rather than styling only one input layer. New Customer also retains the shared filled-button shadow/highlight. Card-action skeletons mirror the same theme radius; compact dimensions and the tinted action design remain intact.

- The round mobile filter button keeps MudButton overflow clipping. Its 16px active-settings counter is circular and inset 4px from the top/end so it stays inside the rounded edge; do not position the counter at the square corner of the 44px button.

- The user explicitly removed the inventory search shortcut from the enterprise AppBar. Keep the header free of that search button; inventory navigation and search within individual pages remain available.


## Responsive customer editor dialogs (2026-10-27)

- The user requested MeetFlow's native responsive dialog model, beginning with CustomersList add/edit. The active reference is `D:/source/Municipality/MeetFlow/src/Client/MeetFlow.Client/Components/MfDialog.razor` and its CSS/`meetflow-modal.js`, not the legacy MudDialog components. GoldEx's shared implementation is `Components/Dialogs/ResponsiveDialog*` plus `Services/ResponsiveDialogService` and RCL `js/responsive-dialog.js`. Desktop panels are centered, 610px wide; through 640px they slide from the bottom with 8px gutters, safe-area padding, pinned header/footer and internally scrolling fields. Forms/validation stay MudBlazor. The module respects reduced motion, visual viewport/keyboard changes, modal scroll lock, Escape/backdrop busy guards and focus restoration.
- `ResponsiveDialogHost` is mounted once in BaseLayout. Its one native dialog stays a nonmodal `display: contents` presentation host while idle, then enters the native top layer with `showModal()` while sessions exist. The same MudPopoverProvider and MudSnackbarProvider DOM/instances stay mounted inside this root for its entire lifetime. Do NOT recreate these providers when opening, nesting or closing a form: MudBlazor 9.5 retains references to popover DOM nodes and recreating the provider leaves subsequent card menus at -9999px. No DOM teleporting or z-index workaround is used. Popovers use fixed positioning while the root is modal, outside the animated panel; inactive hosts retain normal page positioning and semantics. The legacy MudDialogProvider remains for unmigrated workflows.
- Sessions form a stack of keyed editor components; inactive parent forms are hidden/inert but retained. One top modal shows the active session. Opening account editors/confirmation from a native customer editor uses this same stack and returns to the parent's draft/focus. Closing a parent cancels descendants; route navigation cancels all sessions, including a pending busy one. Register ResponsiveDialogService as scoped in both AddClientComponents and server AddClientServices.
- CustomersList.OnCreate (including the existing two-stage `?create=true` handoff) and OnEdit now use ResponsiveDialogService. Other customer-editor callers inside invoices, reporting/home and inventory retain their existing MudDialog service contracts. `EditorDialogFrame` renders native content/actions when a ResponsiveDialogSession is cascaded, or a MudDialog wrapper for legacy callers. Keep those compatibility paths until their workflows are explicitly migrated.
- Customer and financial account editors preserve API requests, FluentValidation, generated customer codes, ReturnModel and SubmitIndependently behavior. Save guards and session Busy prevent duplicate submissions or dismissing a pending save. Editing from the native directory starts with EditorDraft.Copy, including nested accounts and decimal values; cancel must not mutate card data. Nested account changes remain part of the customer draft until the customer is saved. Model initialization is guarded against parent busy-state rerenders so codes/price units/account fields are not reset while editing.
- Verification: `node --test scripts/tests/responsive-dialog.test.mjs` covers modality, Tab wrapping, Escape/popover policy, scroll/viewport cleanup and nested focus. `dotnet test src/Tests/GoldEx.Tests.Server/GoldEx.Tests.Server.csproj --filter FullyQualifiedName~ResponsiveDialogTester` covers busy/cancel/results, descendant cleanup and deep draft isolation. Browser checks must include dropdowns inside the modal, card menus after repeated closes, invalid-form feedback, nested account cancellation, the clean-URL quick-create handoff, and mobile/desktop geometry. Do not save or delete real customers/accounts merely for UI testing.
- Live verification passed at 320x640, 414x900 and 1440x1000: mobile panels retain 8px side/bottom gutters, short-screen fields scroll while actions stay visible, and desktop panels center at 610px. Dropdown Escape, nested-account cancellation, draft isolation, clean-URL creation, repeated card-menu positioning and scroll/focus restoration were checked without saving customer/account records. The seven browser-module suites passed 49 tests; ResponsiveDialogTester passed all four cases, including nested price-unit DTOs and decimal preservation.

## Repository handoff verification (2026-10-28)

- The existing development branch is named `Dev` and tracks `origin/Dev`; preserve its capitalization instead of creating a second lowercase `dev` branch.
- Final feature verification passed all 49 browser-module tests and all 39 .NET tests selected by `NotificationPush`, `ResponsiveDialogTester` and `CustomerOverviewTester`. The .NET verification reused the previously compiled output with `--no-build --no-restore`.
- NotificationList no longer has isolated CSS after the drawer redesign; its obsolete empty stylesheet was removed. Notification styling remains in the existing shared appearance stylesheet.


### Mobile Settings Access (GoldEx Enterprise)
- The redesigned enterprise shell renders its mobile settings entry in `src/App/Client/GoldEx.Client.Components/Layouts/Components/AppBar.razor` for authenticated users only; the profile avatar is not part of the top bar. Account management stays in `NavMenu.razor`.
- `Layouts/Components/Settings.razor` owns the accessible native mobile bottom-sheet dialog; `SettingsSheetLink.razor` renders links, and `wwwroot/js/settings-sheet.js` handles modal focus, backdrop/Escape dismissal and viewport changes. CSS lives in `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css`.
- Keep settings destinations and role-based visibility synchronized with `src/App/Client/GoldEx.Client/Pages/Settings/Shared/SettingsNavMenu.razor`. The breakpoint is below 960px, matching the hidden desktop settings sidebar.

### Active Store Selection in NavMenu (2026-10-28)
- The enterprise active-store selector lives directly in `src/App/Client/GoldEx.Client.Components/Layouts/Components/NavMenu.razor`, inside the scrollable navigation area. Do not revive the removed `StoreSwitcher.razor` / `StoreSelectorDialog.razor` modal pattern.
- NavMenu loads `IStoreService.GetUserStoresAsync` once, retaining the current store for the Vitrine navigation link. Its inline expander presents only the stores accessible to the signed-in user, with current/default badges and accessible button states. The store list must not trigger the NavMenu route-click handler or prematurely close the mobile drawer.
- Switching calls `IStoreService.SwitchStoreAsync`; the existing cookie-backed context requires a full reload, but do it **only in the successful callback**. On failure, leave the current context intact and allow retry. CSS and reduced-motion behavior are in `src/App/Server/GoldEx.Server/wwwroot/css/appearance.css`.

### Native navigation shell, reference to MeetFlow (2026-10-28)
- The live GoldEx enterprise `NavMenu.razor` already uses native `<nav>` and built-in Blazor `NavLink` plus `ShellIcon`. It is **not** `MudNavMenu`. MeetFlow's active `Layout/MainLayout.razor` uses the same native navigation pattern; its old `Layout/Components/NavMenu.razor` with `MudNavMenu` is legacy code, not the active product sidebar.
- The desktop settings submenu `src/App/Client/GoldEx.Client/Pages/Settings/Shared/SettingsNavMenu.razor` now also uses native `NavLink` / `ShellIcon`, preserving all 17 routes and administrator-only entries. Keep its destinations/permissions synchronized with the mobile settings sheet. Both menu styles live in the enterprise `appearance.css`.
- Mobile top-bar logo spacing from the hamburger is `margin-inline-start: 2px` on `.gex-mobile-brand` only below 960px, using RTL logical properties.
- The main NavMenu fetches accessible stores after the first interactive render (not while rendering the initial links); this avoids delaying the initial native navigation. The drawer calls the `navigation.js` update function only when its open state changes rather than on unrelated rerenders, preserving focus trap, inert background, Escape, and scrim behavior.

### Invoice directory native refactor (2026-10-28, issue #243)
- Enterprise invoice list entry `Pages/Invoices/List.razor` wraps `Components/InvoicesList.razor` in the existing main layout without redundant breadcrumbs/headings; invoice page content uses its own responsive layout. The same list component remains usable from customer dialogs through `CustomerId`.
- The main list is a semantic native HTML table on desktop (>=960px) and purpose-designed cards below 960px; neither native table/card rendering nor its next/previous/page-size controls depend on `MudTable` or `MudPagination`. UI CSS is in `InvoicesList.razor.css`; `MudDateRangePicker` is retained for the existing Persian date filtering UX, not for table rendering.
- Preserve `IInvoiceService.GetListAsync`, filtering by date/search/type/scale/status and optional `CustomerId`, sort labels, PDF/view/edit/remove routes and the removal confirmation dialog. `PersistState` keeps URL keys `page`, `size`, `search`, `status`, `type`, `scale`, `start` and `end`. Previous/next pagination and 10/14/25/50 rows per page use server-side Skip/Take; changing filters/page size resets the page.
- Expand rows with an explicit accessible toggle. `InvoiceDetails.razor` provides its own native HTML tables and independent 8-item pagination for related financial transactions and stock movements, using existing `ITransactionService` and `IInventoryStockService` endpoints. Do not modify invoice posting/business logic during list UX changes.
- Both desktop table and mobile cards are present in responsive markup, so `Server/wwwroot/js/invoice-details.js` uses `IntersectionObserver` to activate/fetch details **only for the visible component instance**. This prevents doubled detail/CustomerRemaining network requests. The observer must be disconnected and JS/.NET references disposed when an expanded row collapses or navigates away.
- Keep current and posting-time customer balances lazily inside expanded details; do not mount a `CustomerRemaining` request for every visible invoice. Run `node --test scripts/tests/invoice-directory.test.mjs` and `node --test scripts/tests/*.test.mjs` for UI structural and visibility-observer regression checks; build `GoldEx.Client.csproj` after Razor/code-behind changes.

### Invoice directory matches Customers header and filters (2026-10-28, issue #244)
- The customer directory in `src/App/Client/GoldEx.Client/Pages/Customers/Components/CustomersList.razor` and its isolated stylesheet are the visual/behavioral source of truth for the invoice directory toolbar. Keep the exact `directory-heading`, `directory-title`, `directory-count`, `directory-filters`, `directory-toolbar`, `directory-search`, `directory-filter`, `directory-sort`, `directory-sort-toggle`, and `directory-applied-filters` markup/classes and responsive dimensions synchronized with the Customers component.
- Both pages use `MudTextField` with Clearable/Immediate/500ms debounce for search; `MudButton` with Tune icon for the filter dialog; `MudSelect` for sorting; and `MudIconButton` for sorting direction. Invoice business filters (date, type, trade scale and payment status) are in its existing `InvoiceFiltersDialog` rather than four misaligned desktop inputs. Desktop native HTML invoice table and mobile cards remain independent of MudTable.
- The filters wrapper is sticky below AppBar at `calc(var(--gex-appbar-height, 70px) + env(safe-area-inset-top, 0px) + 8px)`, exactly like Customers; breakpoint <=959px uses search + 44px filter button with sort hidden. `Pages/Invoices/List.razor` uses the same `MudContainer MaxWidth.ExtraLarge` and desktop-only Breadcrumbs wrapper as Customers.
- Invoice pagination uses only previous, current page/total pages and next plus an unobtrusive page-size dropdown (10/14/25/50), with no verbose X-to-Y-of-Z caption. Total invoices appear in the heading badge, matching the Customers count badge. Test parity in `scripts/tests/invoice-directory.test.mjs`.

### Readable invoice/customer typography and Gold palette contrast (2026-10-28, issue #245)
- Customers and invoice directory markup use built-in Mud typography classes (`mud-typography-h5` for page headings, `mud-typography-subtitle1` for customer identities, `mud-typography-body2` for native invoice table). Keep CSS copy/body typography near the standard body1/body2/caption scale with font weights 400/500/600 instead of custom 700-900 and tiny text that looks blurred in Persian.
- Gold palette tokens live in `src/App/Client/GoldEx.Client.Components/Themes/ColorPalettes.cs`. Gold light primary `#8C650F` (5.27:1 on white, 4.75:1 on warm background) and secondary `#8F6510` are deep gold text accents with white button text. Gold `TextSecondary` is neutral dark `#60676C`, never light golden copy; success/info/warning/error light tokens also require >=4.5:1 text contrast. Gold dark bright semantic buttons need explicit dark contrast text to prevent invisible white-on-gold labels. Maintain tests in `scripts/tests/gold-typography-contrast.test.mjs`.
- Invoice desktop rows must show `CustomerRemaining` directly in their customer-balance column, matching the pre-refactor invoice list; never replace the value with a 'view balance' action that forces row expansion. Preserve `EnableManualSlide` and the old current-balance service behavior. Expanded `InvoiceDetails` still offers historical balance at issuance alongside transaction/stock details. Do not add a second hidden-mobile CustomerRemaining instance or eager historical balance request per row.

### Warm Gold accents, aligned directory badges and document-sticky invoice table (2026-10-28, #246)
- Keep the light Gold semantic text palette WCAG AA on white and warm backgrounds: primary amber `#986400`, secondary `#8B640E`, success `#087D44`, error `#C12D4B`. The brighter metallic fill `#D1A42D` with dark text `#251D0D` is intentionally scoped by `html[data-gex-palette="Gold"]:not([data-gex-mode="dark"])` to filled primary controls, active native menus and create affordances in `Server/wwwroot/css/appearance.css`. Do **not** set global primary to the bright fill: that would make ordinary links/text on white unreadable. Other palettes/dark mode retain their own semantic colors.
- Both `CustomersList.razor.css` and `InvoicesList.razor.css` align headings and inline count badges using flex, a consistent logical gap and no redundant badge margin; the badge can wrap as a unit on extremely narrow viewports.
- Native invoice desktop table headers stick beneath the existing sticky filter bar and AppBar during **document scrolling**. Never reintroduce `overflow: hidden` or `overflow-x: auto` on `.invoice-results` or `.invoice-desktop-table`: those ancestors prevent `position: sticky` from following document scroll. CSS sticky top uses `--invoice-filters-block-size` set by `Server/wwwroot/js/invoice-sticky-header.js` through a `ResizeObserver` so wrapped active filter chips cannot overlap the column header. Dispose the observer and JS references. When `InvoicesList` is inside a customer dialog, use dialog-relative sticky offsets instead of AppBar height.
- Regression checks: `node --test scripts/tests/invoice-sticky-header.test.mjs`, `node --test scripts/tests/gold-typography-contrast.test.mjs`; full suite `node --test scripts/tests/*.test.mjs`.

### Gold active-menu/button contrast and invoice customer phone alignment (2026-10-29, #247)
- Light Gold selected native navigation (`.gex-navigation-menu a.active`, settings nav), filled primary/secondary actions, active segmented tabs and create circle use warm amber background `#996500` with white text/icons `#FFFFFF` (contrast ~4.98:1); hover selected menu uses `#9D6800` with white text (~4.76:1). Keep these styles strictly scoped to the light Gold shell in `Server/wwwroot/css/appearance.css`. Do not change the global semantic primary or override dark Gold with white text on its brighter primary.
- `InvoicesList.razor` renders the phone number in both desktop and mobile templates with `class="invoice-customer-phone" dir="ltr"`. The grid containers should use RTL inline-start/right alignment and `text-align:right` so phone digits retain their LTR reading direction yet appear directly beneath the customer name, not on the opposite edge. See `InvoicesList.razor.css`. Verify contrast and alignment through `scripts/tests/gold-menu-customer-phone.test.mjs`.

### Skeleton paging and invoice financial/stock timeline (2026-10-29, #248)
- `InvoicesList.razor` shows responsive native desktop-table/mobile-card skeleton markup for **every** server request (`if (_loading)`), not only initial empty loads. `LoadInvoicesAsync` invokes `StateHasChanged` and yields before the service request so paging visibly responds at once; stale invoice rows must not remain on screen while navigating. Keep loading/error/empty branches independent and accessible.
- Pagination layout in RTL puts `invoice-page-size` first (visual right) and `invoice-pagination-actions` second (visual left). Allowed page sizes are 10/25/50; default 10. Treat old `?size=14` as default 10, and omit default size from URL. No verbose range count.
- Paid invoice statuses in desktop/mobile use the same solid green `invoice-paid` chip. Background `--mud-palette-success` and foreground `--mud-palette-success-text` respect each theme's correct contrast.
- Expanded `InvoiceDetails.razor` now uses `ol.invoice-event-timeline` for financial transactions and stock movements instead of nested HTML tables. Credit (`TransactionType.Credit`) uses solid success-green chips, debit (`TransactionType.Debit`) solid error-red chips, and amounts follow the same semantic colors. Stock In uses entry-green and Out exit-red. Never derive financial transaction debit/credit semantics from positive/negative amounts; use the enum. Each event retains date, description, amount/weight and affected ledger account where applicable. The existing two service endpoints, independent 8-row pagination, lazy `IntersectionObserver` activation and customer historical balances are preserved. Both tab IDs are unique per component instance for desktop/mobile parallel DOM.
- Detail page navigation also yields before API calls and shows event-card skeletons. Do not reintroduce `MudTable` or `MudPagination` to these sections. Structural test: `scripts/tests/invoice-paging-timeline.test.mjs`, full suite: `node --test scripts/tests/*.test.mjs`.

### Invoice mobile balance accents and navigation typography (2026-10-29, #249)
- Mobile invoice cards (NOT desktop table) add `UnpaidClass(invoice)` from `InvoicesList.razor.cs` to their root. Scoped mobile CSS applies extremely restrained 5% radial hue plus a 3px `border-inline-start` for `is-credit` (green) and `is-debit`/`is-overdue` (red); paid and zero/neutral balances retain plain surfaces. Derive statuses only from existing invoice logic, not sale/purchase badge colors or an arbitrary customer currency.
- Customer card balances are loaded asynchronously by `CustomerRemaining`, may represent several currencies, and rotate. Never tint an entire customer card based on the presence of a credit/debit value in any single currency; only add a customer-level status when a specifically defined, reliable aggregate business rule is available.
- Mobile invoice header scale/type chips are aligned and minimally 26px tall, the filled paid chip in the amounts grid is reduced to a compact 28px visual. Keep chips readable on 320px screens and in dark/light modes without inflating their typography or competing with the amount.
- `appearance.css`: sidebar invoice create control and active `gex-navigation-menu` items use standard weight 500, as do selected mobile bottom navigation links and bottom create. Selection remains distinguished by its fill and icons, not by heavy bold text. Invoice header create control uses a desktop-only -16px logical inline-end margin to align visually with the search/filter card's outer edge; do not shift the mobile control out of viewport.
- Tests: `node --test scripts/tests/invoice-mobile-tone.test.mjs` alongside full `node --test scripts/tests/*.test.mjs`.

### Invoice paid chip foreground specificity and both-theme contrast (2026-10-29, #250)
- Mobile `InvoicesList.razor.css` had a broad `.invoice-card-amounts > div span` label rule (specificity higher than `.invoice-paid`) that painted the green paid-status chip's *text* muted gray in light/dark themes. Only amount labels may receive secondary text color; use `.invoice-card-amounts > div > span:not(.invoice-paid)` to exclude the status chip. Paid chip text must explicitly use `--mud-palette-success-text` with sufficient specificity and the ShellIcon SVG must inherit `currentColor`. Changing only the palette or background is insufficient when a more-specific rule overrides the foreground.
- Gold light theme success `#087D44` requires `SuccessContrastText = "#FFFFFF"`. Gold dark success `#3dcb6c` uses `SuccessContrastText = "#151D23"`. Both must exceed WCAG 4.5:1 for normal text. Verified in a real Chrome browser by rendering the source CSS rules and inspecting computed text/SVG/background colors for both themes. This isolated component reproduction is **not** a logged-in end-to-end invoice page screenshot.
- Regression guard: `scripts/tests/invoice-paid-chip-contrast.test.mjs` checks that the broad selector never returns and both contrast tokens are present. Run complete Node UI suite after any change to invoice chip or mobile label CSS.

### Premium desktop liquid-glass navigation dock (2026-10-29, issue #251)
- Desktop navigation uses the existing `Drawer.razor` / `NavMenu.razor` and native `ShellIcon` links, not MudDrawer. From width 960px the former fixed 238px sidebar becomes the `gex-liquid-dock` 64px translucent/frosted capsule, positioned on the RTL right below AppBar, with glow and liquid-like individual pill expansion on hover/keyboard focus. The `gex-desktop-brand` in `AppBar.razor` takes over the desktop GoldEx logo, while the existing `gex-mobile-brand` and mobile drawer keep their prior behavior.
- Only desktop `appearance.css` media queries resize the workspace (no 238px fixed sidebar) and render the dock. The `gex-menu-scroll` container has **no scrolling** on desktop. `src/App/Client/GoldEx.Client.Components/wwwroot/js/liquid-dock.js` binds via Drawer and calculates visible native links based on the real menu viewport height minus the asynchronously mounted store picker. Remaining links are presented as accessible **cloned** proxies under the More button; proxy clicks delegate to the original NavLink rather than moving or altering Blazor-owned DOM. A MutationObserver watches the full nav viewport for store changes and authorized links; a ResizeObserver watches capacity. Release both observers/listeners in `dispose`. More flyout supports Escape, outside click, route navigation and focus return; multi-column layout prevents inner flyout scrolling when several links overflow.
- Keep active/account/store switch navigation and external vitrine links. The store picker collapses to its logo but expands on focus/hover; account identity collapses to an avatar, with logout available on focus/hover, and `LicenseStatusBanner` remains in the More panel on desktop. Under 960px, `gex-dock-more` and its flyout use `display:contents` so the normal mobile license area still exists; `liquid-dock.js` must not leave the mobile license subtree inert or `aria-hidden`. Keyboard navigation has focus-visible pills, fully accessible button labels and reduced motion/transparency fallbacks.
- Verify `node --test scripts/tests/liquid-dock.test.mjs` and full UI tests. Structural Razor refactor was built using the Client.Components project. Do not claim authenticated live screenshot validation merely from Node tests; a dedicated isolated browser preview failed to connect on 2026-10-29.

### Liquid Dock seamless glass, logout and active-route movement (#252, 2026-10-29)
- The Liquid Dock now uses ONE gex-dock-glass-frame > gex-dock-glass to draw and animate the entire glass outer contour via dockSilhouette(...) CSS clip-path: path(...). An independent gex-dock-liquid-lobe must never be restored: it caused white seams and an uncovered logout region. gex-dock-active-drop remains a separate golden navigation marker with the existing Blazor active-class sync and More fallback.
- ShellIdentity.razor logout includes a visible desktop text span adjacent to avatar; CSS keeps avatar/logout in one hover and focus-within area with no dead zone. Existing mobile logout icon remains unchanged.
- Active store continues using _currentStore.LogoUrl inside circular logo, falling back to store icon only if missing. Store trigger stays enabled for a single store so hover/focus can show details; ToggleStoreList returns for Count <= 1 without opening choices. Desktop chevron uses additional inline-end spacing.
- Run node --test scripts/tests/liquid-dock.test.mjs and the full suite. Component build succeeded. An HTTP 200 does not constitute verified signed-in browser rendering; the dedicated browser failed to connect.
### One-piece dock silhouette (#253, 2026-10-29)
- Desktop: the 272px backdrop-filtered gex-dock-glass underlay is clipped to a SINGLE continuous path: the normal 64px rail is x=208..272 and a local shoulder bends LEFT around the hovered row. The entire desktop aside is transparent. CSS path(...) interpolation is supported and animates in Chrome 156; all path branches share the same command sequence. The old separate glass lobe was removed, and there is a narrow-rail fallback before JS starts. Only desktop >=960px uses the shape; mobile drawer does not.
- The bottom avatar/logout case must use bottomRow=true, where lower control points collapse into a straight wall and the bottom-left corner shifts LEFT. Without this, the bottom protrusion self-intersects and produces two strips. gex-dock-is-account tints the ENTIRE one-piece background pale red; the 206px signout action stays transparent, with text and icon visible, rather than painting another partial red pill.
- Chrome headless static previews showed the middle bulge, revealed an initial bottom double-fold which was corrected, and confirmed a continuous bottom extrusion in a new 1200px desktop render. This is not an authenticated browser test. Guard the no-lobe and bottom contour with scripts/tests/liquid-dock.test.mjs. Temporary previews were deleted.
### Content-fit rounded Liquid Dock and isolated gold primary action (#254, 2026-10-29)
- Desktop gex-dock-glass clips one 272px-wide surface; dockSilhouette(...) now forms a fully rounded ~23px left cap at the hovered row, with shallow shoulders. Never restore a fixed 220px width.
- liquid-dock.js measures natural glyph width with document.createRange()/getBoundingClientRect() (ignoring former label min-width 123px), adds icon/padding/gap allowances and sets --gex-item-expanded-width per original anchor. CSS links use min-width:0 labels and the variable on hover. Recompute when document.fonts.ready completes.
- The New Invoice action is an independent GOLD primary pill; hovering it does NOT expand the shared glass silhouette and the label remains white.
- Profile and logout share the identity bounding rect at the bottom. The former H-106 clamp caused the surface to reach the More (...) row. Now the expansion begins near the avatar and clamps to H-52 with a short upper shoulder. Logout width is content-fit, not 206px.
- Validate with scripts/tests/liquid-dock.test.mjs and full Node suite. Headless Chrome static visuals were inspected, but authenticated live browser testing is separate. Under project policy do not automatically build for CSS/JS UI-only changes.
### Selected Liquid Dock route must be a FILLED Primary pill on hover (#256, supersedes #255, 2026-10-29)
- The #255 text-only workaround was incorrect: it only recolored the expanded active label, leaving transparent glass underneath and breaking Gold dark mode contrast. Do NOT restore the non-Gold .active:hover > span label-only text-primary override.
- On desktop >=960px, every selected NavLink (.gex-navigation-menu > a.active:is(:hover,:focus-visible)) expands to natural --gex-item-expanded-width as a full opaque background: var(--mud-palette-primary) pill with 999px radius and subtle shadow, with BOTH label and SVG icon using color: var(--mud-palette-primary-text). This is the same UX as the New Invoice button. The small selected droplet remains in the unhovered state. Old transparent background active selectors must be overridden for selected hover/focus. Gold light requires warmer #9D6800 and white text; Gold dark uses LIGHT #DAA520 primary and DARK #151D23 contrast foreground. The other dark palettes also use dark foreground against light primary. Mobile remains untouched.
- In liquid-dock.js, selected hovered native NavLink uses the same glass-outline exclusion as New Invoice: renderHull uses shapeTarget=null while the selected link expands as its own opaque pill, keeping the underlying glass narrow. Inactive links still deform the single translucent glass surface. Preserve active-route animation and overflow.
- Chrome headless using REAL DevTools Protocol Input.dispatchMouseEvent (not synthetic JS events or text-only tests) verified computed active hover background/contrast label and SVG for all SIX themes (Gold, Blue, Green, Purple, Red, Orange) in light and dark: 12/12 successful. Gold light background rgb(157,104,0) with white foreground; Gold dark background rgb(218,165,32) with rgb(21,29,35) dark text/icon. Temporary CDP fixture and script removed. This was a browser CSS interaction reproduction, not an authenticated full GoldEx page. scripts/tests/liquid-dock.test.mjs guards the filled pill and glass exclusion.
### Enterprise AppBar desktop breadcrumb migration and Persian clock (#257, 2026-10-29)
- AppBar.razor now contains a single custom RouteBreadcrumbs component after desktop/mobile branding and before actions. Clock appears AFTER actions at the left edge of the RTL topbar. Never add MudBreadcrumbs to the AppBar. On desktop >=960px .goldex-page-container .mud-breadcrumbs is hidden by appearance.css; existing page-local MudBreadcrumbs markup intentionally remains for mobile (user deferred its redesign).
- RouteBreadcrumbs.razor follows NavigationManager.LocationChanged and unsubscribes on Dispose; route hierarchy uses known ClientRoutes prefixes, strips query/hash, names ancestors and current destination, and does not expose raw GUIDs/IDs as labels. Nested Invoice, Inventory, Reporting, Settings, Finance, User Account and other routes are mapped. Dynamic /invoices/set distinguishes create and edit. Extend its labels with new routes. The trail is ellipsized at desktop widths, animated modestly and respects reduced motion.
- Help.razor may render nothing; previously its empty .gex-topbar-help div consumed an entire flex gap. Desktop/tablet display:contents collapses that wrapper; the actual desktop Help button uses order:-1 so it appears to the RIGHT of AppearanceControls in RTL. Small mobile widths continue hiding Help.
- Clock.razor uses PersianCalendar to format weekday/day/month/year (e.g. چهارشنبه ۸ اردیبهشت ۱۴۰۵) and HH:mm clock on the line below, Persian digits, updating each minute with a disposed timer. Decorative calendar icon removed, font size raised.
- Tests: scripts/tests/appbar-route-trail.test.mjs validates layout order, route updates, safe IDs, the 47 existing mobile-preserved page breadcrumbs, Help gap and Jalali date. Full Node suite passed 99/99; GoldEx.Client.Components build passed 0 warnings/errors. Pre-existing Liquid Dock #256 CSS regression was scoped to its own CSS section so unrelated later style rules do not break it.
- Task-specific skill: .matemcp/skills/appbar-route-trail/SKILL.md; enterprise release 1.0.105. Local commit only, no push/deploy.
### GoldEx desktop shell fixes: RTL arrow, account hull, store dismissal, release menu, symmetric spacing (#258, 2026-10-29)
- Desktop AppBar custom RouteBreadcrumbs uses explicit left-pointing arrow (U+2190) with CSS direction:ltr;unicode-bidi:isolate to prevent RTL mirroring. Old left-angle glyph mirrored incorrectly. Page-local MudBreadcrumbs remain for mobile.
- Logout is still within the one-piece glass profile silhouette. liquid-dock.js adds 44px account hull clearance; desktop signout is inset by 12px and limited to max 174px with overflow hidden so label/icon fit, and the shape does not engulf the More (...) control. Tests assert dynamic contour, not brittle fixed coordinates.
- Active-store list stays Blazor state. navigation.js captures document pointerdown and keydown Escape, ignores inside clicks, invokes original store trigger.click() for outside/Escape so UI state remains synchronized. Disposal removes listeners; mobile and desktop both work.
- The manual 'تغییرات برنامه' action moves from Home into authenticated NavMenu as ReleaseNotesMenuItem.razor using ReleaseHistory dialog, IAppReleaseService and IVersionReleaseStore. Home still automatically announces new versions. Menu item is a button and liquid-dock.js allLinks includes gex-nav-release for sizing and overflow delegation.
- At desktop >=960px main body --gex-content-gap:26px matches Dock inset 26px. Under 960px keep 12px. Home gex-home-breadcrumb remains visible under 960px, not just below the MudBlazor tablet breakpoint.
- New shell-polish.test.mjs and expanded navigation.test.mjs guard these behaviors. Liquid Dock tests verify the one-piece account contour using dynamic geometry. 106/106 Node tests passed; Client.Components builds 0 warnings/errors. Enterprise release 1.0.106. No push/deploy.
### Enterprise UI follow-up: restore chevron, More-only release notes, responsive directory filter dialogs (#259, 2026-10-29)
- AppBar route-trail separator uses existing ShellIcon Name="chevron-left" SVG instead of plain left arrow or mirrored bracket. Nested gex-shell-icon keeps direction:ltr/transform:none to avoid RTL flipping.
- Authenticated NavMenu owns a single ReleaseNotesMenuItem button. Desktop Liquid Dock refresh() treats button.gex-nav-release as forceOverflow: excludes it from routeLinks/visible slots, always includes it in extras, assigns data-dock-overflow=true, and CSS hides the original. More renders one proxy with RTL flex-start alignment. Clicking command buttons must NOT move active NavLink droplet; only a[href] navigates. Mobile retains native button.
- CustomerFiltersDialog and InvoiceFiltersDialog now use EditorDialogFrame + ResponsiveDialogService Show (same native desktop modal/mobile bottom sheet as customer Editor). The Cascading ResponsiveDialogSession owns Close/Cancel. Parent callers await session.Result and apply the result. Responsive MudGrid layout. CSS gex-filter-mobile-sort hides sort controls at >=960px only. Both mobile filter forms now handle sort label and direction, including invoice query wiring. Desktop toolbar remains authoritative, and clearing customer filters preserves desktop sorting.
- Tests scripts/tests/directory-filter-dialogs.test.mjs and updated shell-polish.test.mjs guard chevron, More dedup/alignment, native editor modal, sorting. 110/110 Node tests passed, GoldEx.Client compiled zero warnings/errors. Chrome actual appearance.css at viewport 1384/1034px desktop and 604/500px mobile verified correct conditional sort display and hidden original/visible flex-start aligned More proxy. Release 1.0.107; no push/deploy.
### Notification drawer and calculator tabs typography normalization (#260, 2026-10-29)
- The shared appearance.css owns notification drawer and calculator workspace typography. Previously notification heading h2 was 800, summary labels/action buttons 700, notification titles 750, calculator tabs 650/800, and calculator context title 800. Non-standard intermediate weights 650 and 750 plus many 700/800 labels made Persian text look cramped and blurry.
- Use a restrained semantic hierarchy: 400 for regular body, quiet buttons and inactive tabs; 500 for medium section labels, unread notification titles, primary buttons, active tabs and calculator context title; 600 only for notification drawer main heading. Read notifications have 400-weight titles; unread ones retain 500 and primary-tinted card background/border, NOT heavy bold. Calculator tab selection still uses Primary color/underline and transitions. Main notification heading is 19px/600, body at least 13px, calculator tabs 14px desktop / 13px mobile, context description 13px/400. Preserve theme variables for light/dark; avoid broad weight overrides inside calculation forms/results where financial numbers may require emphasis.
- Regression suite scripts/tests/notification-calculator-typography.test.mjs validates hierarchy, contrast and active states. 114/114 Node UI tests passed. Actual Chrome computed styles with appearance.css at 1350px desktop and 500px mobile, light/dark, confirmed heading weights 600/500/400, calculator tabs 400/500 and descriptions 400 in four modes. Static browser CSS check only, not authenticated live app E2E. Do NOT run dotnet build for CSS-only change per project policy. Enterprise release 1.0.108; GitHub issue #260. No push/deploy.
### Minimal unread-only notification drawer (#261, 2026-10-29)
- GoldEx enterprise notifications UI is in Notifications.razor, NotificationList.razor, their code-behinds and the shared appearance.css. The drawer is now unread-only: LoadNotificationsAsync still queries the existing API and maps _items = result.Where(x => !x.IsRead).ToList(), with _unreadCount = _items.Count kept synchronized with AppBar via UnreadCountChanged. Polling every 1 minute, reloading when opened, mark-one-read, mark-all-read, contextual invoice actions stay intact.
- Only a compact header (اعلان‌ها, unread-count chip, conditional خواندن همه, close) and unread notification list remain. Remove redundant summary, manual refresh, list toolbar, per-item جدید chip, footer, long descriptions and giant empty state. An empty inbox shows one short line and a compact check icon. Push activation is a brief row ONLY when supported/configured/not subscribed and permission not denied; it disappears immediately after subscription succeeds. Show feedback only on unsuccessful/unsubscribed attempts. Do not restore Test Push or Disable Push UI: handlers have been removed from Notifications.razor.cs but underlying JS opt-out/test compatibility remains untouched.
- Styles use theme variables and restrained 400/500/600 typography. Preserve message actions, accessibility (ARIA, Escape/backdrop, focus), and mobile dialog.
- Tests: scripts/tests/minimal-notifications.test.mjs guards unread-only state, subscription conditional, UI removal and preserved handlers. notification-calculator-typography.test.mjs updated. Full Node suite 118/118 passed. No dotnet build for minor UI/code-behind removal per project build policy. Enterprise release 1.0.109, issue #261; local Dev commit only, no push/deploy.