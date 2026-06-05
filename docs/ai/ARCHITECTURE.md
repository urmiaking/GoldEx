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
