# GoldEx AI Agent Instructions

Read these documents before generating code:

- ./docs/ai/ARCHITECTURE.md
- ./docs/ai/SERVICE_ARCHITECTURE.md
- ./docs/ai/DOMAIN_KNOWLEDGE.md
- ./docs/ai/DEVELOPMENT_GUIDE.md
- ./docs/ai/TOOLS.md

Every time that you learn something new about the project update the AGENTS.md file with the new information.
This file should be the single source of truth for all AI agents working on the project. Always refer to this file before generating code or making architectural decisions.

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
