# GoldEx Architecture Guide

## Architectural Style

GoldEx follows:
- Clean Architecture
- Domain-Driven Design (DDD)
- Modular layered organization

---

## Layer Responsibilities

### Domain Layer
Contains:
- Aggregates
- Entities
- Value Objects
- Domain Events

Must remain framework-independent.

### Application Layer
Contains:
- Use cases
- Validation
- Orchestration
- Background jobs
- Transaction coordination

### Infrastructure Layer
Contains:
- EF Core
- SQL Server persistence
- External APIs
- Price providers
- SMS integrations

### Presentation Layer
Contains:
- Controllers
- API endpoints
- Authentication setup
- Swagger
- Middleware
- Dependency injection

---

## Client Architecture

Blazor WebAssembly + MudBlazor.

Structure:
- Pages
- Components

Goals:
- Reusable UI
- Fast rendering
- Responsive design

---

## Calculator App Philosophy

GoldEx Mini is:
- Offline-first
- Lightweight
- Client-side focused
- LocalStorage powered

Internet is required only for:
- Initial installation
- First pricing synchronization

---

## Persistence Principles

SQL Server + EF Core.

Requirements:
- Transaction safety
- Optimistic consistency
- Financial correctness
- High precision decimal handling

---

## Logging & Monitoring

Uses:
- Serilog
- SQL sink
- Health Checks
- Serilog.UI

Monitor:
- Pricing providers
- Database health
- External APIs
- Background jobs

---

## Multi-Tenancy Architecture (Shared Database)

GoldEx has transitioned to a shared database multi-tenancy model:
- **Tenant Scope:** Tenants are represented by the `Store` aggregate root.
- **Tenancy Scoping:** Partitioned entities implement `IStoreFiltered` and have a `StoreId` property.
- **Resolution:** A scoped `IStoreContext` holds the active tenant's `StoreId` and `StoreSlug` resolved from context cookies/user defaults via `StoreResolutionMiddleware`.
- **Global Filters:** EF Core applies dynamic global query filters to enforce isolation: `e.StoreId == CurrentStoreId`. To prevent LINQ translation exceptions against relational databases, the filter must reference a property on the `DbContext` class itself (e.g. `CurrentStoreId`) rather than inline constructor instantiations (like `new StoreId(...)`). EF Core evaluates this property at query execution time and applies the registered `StoreIdConverter` to the parameter.
- **Automatic Assignment:** `GoldExDbContext.SaveChangesAsync` automatically assigns `StoreId` for new entities if not explicitly provided.
- **Unique Indexes:** Database-level unique constraints must always include `StoreId` (e.g. `{ StoreId, Barcode }` or `{ StoreId, InvoiceNumber, InvoiceType }`).
- **Reports & Static Assets:** Reports are scoped using file suffixes (e.g., `InvoiceReport_{storeSlug}.repx` with fallback to `InvoiceReport.repx`). Logos are written/read at `logo_{storeSlug}.png`.
- **Administration Restriction:** Report lists and designers are restricted exclusively to the `Administrators` role.

