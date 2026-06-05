# GoldEx Development Guide

## Technology Stack

- .NET 10
- ASP.NET Core
- Blazor WebAssembly (Auto rendering with SSR)
- MudBlazor
- Entity Framework Core
- SQL Server
- Serilog
- Docker

---

## Frontend Standards

- Use MudBlazor consistently
- Prefer reusable components
- Avoid duplicated layouts
- Keep UI responsive
- Use strongly typed models

---

## Backend Standards

- Keep controllers thin
- Use validators for input validation
- Use async database operations
- Respect separation of concerns

---

## Performance Priorities

Optimize:
- Large inventory queries
- Reporting operations
- Pricing updates
- Dashboard rendering
- Barcode scanning workflows

---

## Security Requirements

Use:
- ASP.NET Core Identity
- 2FA
- Passkeys