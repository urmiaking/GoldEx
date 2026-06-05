# GitHub Copilot / AI Coding Assistant Instructions

## Project Context

GoldEx is an enterprise-grade jewelry store ERP and gold accounting platform.

Focus areas:
- Financial correctness
- Clean Architecture
- DDD
- High precision calculations
- Maintainability
- Performance

---

## Preferred Patterns

### Use
- Dependency Injection
- Repository pattern
- Specification pattern
- Value Objects
- Strong typing
- Async APIs
- Immutable DTOs where appropriate

### Avoid
- Anemic domain models
- Static mutable state
- Business logic in controllers
- Direct DbContext usage from UI
- Circular dependencies
- Magic strings

---

## UI Guidelines

- Use MudBlazor
- Keep components modular
- Avoid deeply nested components
- Prefer typed parameters
- Keep rendering performant

---

## Financial Logic Constraints

Never simplify:
- Gold weight calculations
- Invoice totals
- Currency exchange logic
- Accounting entries

Precision and auditability are mandatory.

---

## Naming Conventions

Prefer:
- InvoiceAggregate
- CustomerLedger
- GoldTransaction
- PricingProvider
- ProductBarcode

Use meaningful domain terminology.
