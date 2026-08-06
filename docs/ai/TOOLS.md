# GoldEx Development Tools

## Build

```bash
dotnet build GoldEx.slnx
```

> **AI Agent Note:** Do not run `dotnet build` automatically for minor styling/CSS or Razor layout edits. Only execute builds when modifying C# backend logic, APIs, or when requested.

## EF Migration

```bash
dotnet ef migrations add <MigrationName>
```