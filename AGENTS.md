# GoldEx AI Agent Instructions

Read these documents before generating code:

- ./docs/ai/ARCHITECTURE.md
- ./docs/ai/SERVICE_ARCHITECTURE.md
- ./docs/ai/DOMAIN_KNOWLEDGE.md
- ./docs/ai/DEVELOPMENT_GUIDE.md
- ./docs/ai/TOOLS.md

Every time that you learn something new about the project update the AGENTS.md file with the new information.
This file should be the single source of truth for all AI agents working on the project. Always refer to this file before generating code or making architectural decisions.
If any new feature is added, we should add it to releases.json file after implementation

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
