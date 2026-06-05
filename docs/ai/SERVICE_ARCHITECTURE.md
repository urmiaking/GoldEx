# GoldEx Service Architecture Rules

## IMPORTANT: GoldEx Does NOT Use CQRS

Do not introduce CQRS patterns, MediatR handlers, command/query objects, or unnecessary request pipelines unless the surrounding code already uses them for that specific feature.

GoldEx primarily uses:
- Shared service abstractions
- Direct application services
- DTO contracts
- Dual implementations for SSR and WASM execution

Always follow existing nearby patterns.

---

# Blazor Web App Rendering Architecture

GoldEx uses:

- Blazor Web App
- Auto Render Mode
- Server prerendering
- WebAssembly client hydration

Because of this architecture, services execute differently depending on render mode.

---

# Shared Service Pattern

When introducing a new feature/service:

## STEP 1 — Create Shared Contract

Create the abstraction inside:

- `GoldEx.Shared`

This includes:
- Service interface
- Request DTOs
- Response DTOs
- Shared models/contracts

Example:

```csharp
public interface IInvoiceService
{
    Task<GetInvoiceResponse> GetAsync(GetInvoiceRequest request, CancellationToken cancellationToken = default);
}
```

These contracts must remain transport-safe and serialization-safe.

---

## STEP 2 — Server-Side Implementation

Create implementation inside:

- `GoldEx.Server.Application`

This implementation is used during:
- Server-side rendering (SSR)
- Prerendering
- Api controller invoke

The implementation should:
- Use repository abstractions if needed
- Use infrastructure abstractions indirectly
- Optionally use Mapster mapper
- Execute business logic directly

Example responsibilities:
- Query repositories
- Validate rules
- Map entities to DTOs
- Return shared response models

DO NOT:
- Inject HttpClient
- Call APIs internally
- Duplicate controller logic

---

## STEP 3 — Register Dependency Injection

Register the server-side implementation so SSR resolves it directly.

During SSR:
- The component calls the shared interface
- DI resolves the server implementation
- No HTTP request occurs

This is intentional and important for performance.

---

## STEP 4 — Client-Side WASM Implementation

Create another implementation inside:

- `GoldEx.Client.Services`

This implementation is used after:
- WebAssembly hydration
- Client-side rendering

The client implementation should:
- Inject HttpClient and JsonSerializationOptions
- Call backend APIs using ApiUrls
- Serialize/deserialize shared DTOs
- Implement the SAME shared interface

Example:

```csharp
public class InvoiceService(HttpClient client, JsonSerializationOptions jsonOptions)
    : IInvoiceService
{
    public async Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetInvoiceResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}
```

---

## STEP 5 — Create or Reuse API Controller

Inside:

- `GoldEx.Server`

Create or reuse:
- API controller
- Endpoint

The controller should:
- Be thin
- Delegate to application services
- Avoid business logic

---

# Architectural Rule: Follow Nearby Existing Code

Before creating:
- services
- controllers
- DTOs
- interfaces
- records
- components
- validators
- repositories

the agent MUST:

1. Search for similar nearby implementations
2. Follow the exact same:
   - naming
   - folder structure
   - dependency pattern
   - DTO style
   - method signatures
   - validation style
   - API conventions

Do NOT invent new patterns if equivalent patterns already exist nearby.

Consistency is more important than theoretical purity.

---

# Repository Rules

Repositories:
- are implemented in Infrastructure
- are consumed primarily from Application services

Never:
- inject repositories into UI
- access DbContext directly from presentation/client code
- bypass application services

---

# Mapping Rules

GoldEx commonly uses Mapster.

Preferred flow:

Entity -> Mapster -> Shared DTO

Avoid:
- manual repetitive mapping
- leaking entities outside server layers

---

# Controller Rules

Controllers should:
- validate transport concerns only
- delegate logic to application services
- remain thin

Business logic does not belong in controllers.

---

# Critical Goal

The same shared service contract should work transparently in both:
- SSR execution
- WASM client execution

without changing component logic.
