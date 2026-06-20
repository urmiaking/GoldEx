# Multi-Tenancy Transition Tasks

## Phase 1: Domain Core & Tenancy Setup
- [x] Create `Store` aggregate root in `GoldEx.Server.Domain`
- [x] Create `StoreUser` join entity/aggregate in `GoldEx.Server.Domain`
- [x] Create `IStoreFiltered` interface in `GoldEx.Server.Domain`
- [x] Implement `IStoreFiltered` on the partitioned domain aggregates:
  - `Customer`
  - `Product`
  - `ProductCategory`
  - `InventoryEntry`
  - `InventoryExit`
  - `InventoryStock`
  - `Invoice`
  - `InvoicePayment`
  - `LedgerAccount`
  - `Transaction`
  - `PaymentVoucher`
  - `CheckPayment`
  - `MeltingBatch`
  - `BarcodeInquiry`
  - `BarcodeReservation`
  - `Setting`
  - `SmsLog` & `SmsTemplate`
  - `Notification`
  - `Coin` & `CoinInstance`
  - `PriceUnit`
  - `FinancialAccount`
  - `AppLicense` & `LicensePayment`

## Phase 2: Context Resolution & Shared Service
- [x] Create `IStoreContext` interface in `GoldEx.Shared`
- [x] Implement scoped `StoreContext` in `GoldEx.Server.Application`
- [x] Create `StoreResolutionMiddleware` in `GoldEx.Server` (resolves active store from cookie or user default)
- [x] Create `StoreSwitchController` in `GoldEx.Server` with `/api/stores/switch/{storeId}` endpoint

## Phase 3: Database & EF Core Configurations
- [x] Update `GoldExDbContext` to inject `IStoreContext` and apply dynamic global query filters for `IStoreFiltered`
- [x] Update `GoldExDbContext.SaveChangesAsync` to automatically assign `StoreId` for new `IStoreFiltered` entities
- [x] Update EF Configurations to include `StoreId` in all unique index signatures (e.g. Invoices, Products)
- [x] Generate Db Migration (create default store, add `StoreId` columns, set default values for existing rows, configure unique indexes)

## Phase 4: Client Appbar & Store Selector UI
- [x] Implement `StoreSelectorDialog.razor` with a premium glassmorphic card layout displaying store background images/logos
- [x] Modify `AppBar.razor` to show current store info and a button to open the selector dialog
- [x] Integrate store switching with client-side state reloading

## Phase 5: DevExpress Reports & Suffix Separation
- [x] Restrict report settings lists to the `Administrators` role
- [x] Set up report template naming separation with store slugs/suffixes (e.g. `InvoiceReport_{storeSlug}.repx`)

## Phase 6: Integration Testing Setup
- [x] Configure integration tests to use a separate database `GoldEx_IntegrationTest` and verify store-scoping behavior

## Phase 7: Store Management UI & Admin CRUD
- [x] Create Shared DTOs (`GetStoreRequest` & `StoreRequest`)
- [x] Configure admin endpoints and query filters in `ApiRoutes.cs` & `ApiUrls.cs`
- [x] Create Store specifications (`StoresByFilterSpecification`, `StoreByIdAnyStatusSpecification`, `StoreBySlugSpecification`, `StoreUserByStoreIdSpecification`)
- [x] Inherit `IDeleteRepository<Store>` in `IStoreRepository` and `IDeleteRepository<StoreUser>` in `IStoreUserRepository`
- [x] Update `StoreService` (Server) with pagination, admin methods, and reference verification on delete
- [x] Expose admin endpoints in `StoresController` with `[Authorize(Roles = BuiltinRoles.Administrators)]`
- [x] Implement client-side `StoreService` calls
- [x] Update `ClientRoutes` and add the navigation link in `SettingsNavMenu`
- [x] Implement `StoreVm` client View Model
- [x] Create Store List page `Stores.razor` & `Stores.razor.cs`
- [x] Create Store Dialog Editor `Editor.razor` & `Editor.razor.cs` supporting logo/background image uploads with live preview
- [x] Create Store Delete Dialog `Remove.razor` & `Remove.razor.cs`

## Phase 8: Store User Assignment
- [x] Create `AssignStoreUsersRequest` DTO
- [x] Configure admin endpoints and query routing in `ApiRoutes.cs` & `ApiUrls.cs`
- [x] Declare assignment contracts in `IStoreService`
- [x] Implement store-user mapping and transaction logic in `StoreService` (Server)
- [x] Expose assignment endpoints in `StoresController` with `[Authorize(Roles = BuiltinRoles.Administrators)]`
- [x] Implement client-side `StoreService` assignment calls
- [x] Add the "Assign Users" action button to the `Stores.razor` page grid
- [x] Create the `StoreUsersDialog.razor` and `StoreUsersDialog.razor.cs` Blazor modal dialog
