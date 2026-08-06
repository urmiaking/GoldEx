# GoldEx AI Agent Instructions

Read these documents before generating code:

- ./docs/ai/ARCHITECTURE.md
- ./docs/ai/SERVICE_ARCHITECTURE.md
- ./docs/ai/DOMAIN_KNOWLEDGE.md
- ./docs/ai/DEVELOPMENT_GUIDE.md
- ./docs/ai/TOOLS.md

Every time that you learn something new about the project update the AGENTS.md file with the new information.
This file should be the single source of truth for all AI agents working on the project. Always refer to this file before generating code or making architectural decisions.
If any new feature is added, we should add it to releases.json file after implementation.

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
   - Customer transfer pages (`List.razor`) and components (`CustomerTransferList.razor`, `CustomerTransferEditor.razor`) inherit `GoldExComponentBase` and follow standard GoldEx layout patterns (`MudBreadcrumbs`, `MudTable` with custom `MudPagination`, `SendRequestAsync` thread-safe requests, `ValidateAsync`, and `_processing` submit state).

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


