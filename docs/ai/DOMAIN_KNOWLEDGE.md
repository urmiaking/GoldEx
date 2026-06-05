# GoldEx Domain Knowledge

This document provides a comprehensive guide to the business domain of **GoldEx**, detailing the industries, accounting structures, mathematical formulas, and aggregate rules implemented across the solution.

---

## Core Industry Concepts

### 1. Gold & Jewelry Products
The domain model distinguishes between several product types inside [Product](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/ProductAggregate/Product.cs) using the `ProductType` enum:
- **Jewelry (`ProductType.Jewelry`)**: Manufactured gold items containing gemstones, specific workmanship/wages, and unique barcodes.
- **Gold (`ProductType.Gold`)**: Standard manufactured gold items without heavy gemstone components.
- **Molten Gold (`ProductType.MoltenGold`)**: Melted bars or gold blocks resulting from melting scrap/used gold. Typically has assay details (`AssayNumber`, `AssayDate`, and `AssayerId` mapping to a laboratory).
- **Used Gold (`ProductType.UsedGold`)**: Second-hand gold purchased from customers (scrap gold), valued based on weight, fineness, and a deduction rate.

#### Core Product Attributes:
- **Fineness (Carat)**: The purity of gold, typically modeled as a decimal (e.g., `750` for 18-karat gold, `995` or `999.9` for pure gold).
- **Weight**: Measured in grams. High precision is required (up to 3 decimal places).
- **Wage (Workmanship / اجرت)**: The cost of crafting the jewelry. It can be:
  - `WageType.Percent`: A percentage added to the raw gold price.
  - `WageType.Fixed`: A fixed currency amount per gram, which may be adjusted by an exchange rate.
- **Gemstones ([GemStone](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/ProductAggregate/GemStone.cs))**: Individual stones embedded in jewelry. Tracked with attributes: `Carat` (weight), `Color`, `Cut`, `Purity`, and `Cost`.

### 2. Coins
Coins are modeled under [CoinInstance](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/CoinInstanceAggregate/CoinInstance.cs) representing unique physical coins or homogeneous batches:
- **Mint Types (`CoinMintType`)**: Includes standard mintages such as Emami, Bahar Azadi, Half, Quarter, and Gram coins.
- **Packaging Types (`CoinPackageType`)**:
  - `Open`: Unpackaged loose coins. Open coins cannot have package details.
  - `VacuumSealed`: Sealed coins that must carry holographic security markings, serial numbers, and publisher details modeled in `CoinInstancePackage`.

### 3. Scrap Gold & Melting Workflows
To recycle gold, used jewelry purchased from customers is consolidated and melted:
- **Melting Batch ([MeltingBatch](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/MeltingBatchAggregate/MeltingBatch.cs))**: Groups physical scrap items together. It tracks total weight and progresses through the following statuses (`MeltingBatchStatus`):
  1. `Melting`: The initial state where items are being prepared or melted.
  2. `SentToLab`: Sent to a laboratory/assayer customer to test the purity of the resulting molten gold block.
  3. `Completed`: The melting process is finished, producing a certified molten gold bar.
- **Assay Details**: Once tested, the resulting block is registered as a `MoltenGold` product with an official `AssayNumber` and tested `Fineness`.

---

## Financial Calculations & Formulas

All financial calculations are implemented in [CalculatorHelper](file:///d:/source/GoldEx/src/App/Shared/GoldEx.Shared/Helpers/CalculatorHelper.cs) using deterministic `decimal` arithmetic.

### 1. Raw Gold Price
Calculates the base gold value before wages, profits, or taxes.
- **For Used Gold (Scrap)**:
  $$\text{RawPrice} = \text{Weight} \times \frac{\text{Fineness}}{750} \times \text{GramPrice}_{750}$$
- **For Other Products (Jewelry, Standard Gold, Molten Gold)**:
  $$\text{GramPrice}_{24} = \frac{\text{GramPrice}_{750}}{0.75}$$
  $$\text{FinenessRatio} = \frac{\text{Fineness}}{1000}$$
  $$\text{RawPrice} = \text{Weight} \times \text{FinenessRatio} \times \text{GramPrice}_{24} \times \text{Quantity}$$

### 2. Wage (Workmanship / اجرت)
The workmanship cost added to the raw gold value.
- **Percent-based Wage (`WageType.Percent`)**:
  $$\text{WageAmount} = \text{RawPrice} \times \frac{\text{WagePercent}}{100}$$
- **Fixed Wage (`WageType.Fixed`)**:
  $$\text{WageAmount} = \text{WageValue} \times \text{ExchangeRate} \times \text{Weight}$$
  *(If exchange rate is not provided or matches the invoice currency, it defaults to 1)*

### 3. Seller Profit
- **For Used Gold**: Profit is always **0**.
- **For Other Products**:
  $$\text{ProfitAmount} = (\text{RawPrice} + \text{Wage}) \times \frac{\text{ProfitPercent}}{100}$$

### 4. Value-Added Tax (VAT)
- **For Used Gold / Molten Gold**: Tax is always **0**.
- **For Other Products**:
  $$\text{TaxAmount} = (\text{Wage} + \text{Profit} + \text{StoneAmount}) \times \frac{\text{TaxPercent}}{100}$$

### 5. Invoice Item Final Price
- **For Used Gold**:
  $$\text{FinalPrice} = \text{RawPrice} + \text{AdditionalPrices}$$
- **For Other Products**:
  $$\text{FinalPrice} = \text{RawPrice} + \text{Wage} + \text{Profit} + \text{Tax} + \text{AdditionalPrices}$$

### 6. Purchase Cost Price (Equivalent 750 Weight Basis)
Determines the buy-in value of gold purchased from customers or suppliers:
- **Equivalent Weight**:
  $$\text{EquivalentWeight}_{750} = \text{Weight} \times \frac{\text{Fineness}}{750}$$
  $$\text{PricePerGram} = \text{EquivalentWeight}_{750} \times \text{GramPrice}$$
- **Cost Price Calculation**:
  - *Percent Wage*: $\text{PricePerGram} \times (1 + \frac{\text{WageAmount}}{100})$
  - *Fixed Wage*: $\text{PricePerGram} + (\text{WageAmount} \times \text{ExchangeRate})$
  - *No Wage*: $\text{PricePerGram}$

### 7. Scrap/Used Gold Purchase Valuation
Purchased scrap gold is valued considering a fineness deduction (کسری عیار):
- **Effective Weight**:
  $$\text{EquivalentWeight}_{750} = \text{Weight} \times \frac{\text{Fineness}}{750}$$
  $$\text{EffectiveWeight} = \text{EquivalentWeight}_{750} \times \frac{750 - \text{DeductionFrom750}}{750}$$
- **Final Valuation**:
  $$\text{PurchasePrice} = \text{EffectiveWeight} \times \text{GramPrice} \times \text{ExchangeRate} \times \text{Quantity}$$

### 8. Molten Gold Back-Calculations
When buying or selling molten gold, weight can be back-calculated from a target monetary price:
$$\text{Weight} = \text{Round}\left( \frac{\text{InputPrice}}{\frac{\text{Fineness}}{750} \times \text{UnitPrice} \times (1 + \frac{\text{ProfitPercent}}{100})}, \; 3 \text{ decimal places} \right)$$

---

## Double-Entry Accounting & Ledger Architecture

GoldEx enforces double-entry accounting integrity through structured journal ledger accounts and balanced transactions:

### 1. Journal Transactions
- Every financial change is recorded as a [Transaction](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/TransactionAggregate/Transaction.cs).
- **GroupId (Balance Group)**: To ensure that debits equal credits in a double-entry entry, transactions are grouped together by a `GroupId` (a UUID v7).
- **Exchange Rates**: Transactions store the native transaction `Amount` (in the local currency or weight unit) and the calculated `BaseCurrencyAmount` ($\text{Amount} \times \text{ExchangeRate}$).

### 2. Ledger Accounts
Ledger accounts ([LedgerAccount](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/LedgerAccountAggregate/LedgerAccount.cs)) classify financial statuses:
- **System Accounts**: Configured in [SystemLedgerAccounts](file:///d:/source/GoldEx/src/App/Shared/GoldEx.Shared/Constants/SystemLedgerAccounts.cs) (Assets, Liabilities, Equity, Revenue, Expenses).
- **Customer Accounts**: Linked directly to a customer (`CustomerId`) to track their balance.
- **Unit Types**: Accounts track balances in either standard currencies (IRR, USD, etc.) or physical gold weights using `PriceUnitId` where:
  - `IsGoldBased => UnitType is Gold18K or Mesghal`

### 3. Inventory Exits and Expenses
When stock leaves the warehouse outside of a sale, the [InventoryExit](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/InventoryExitAggregate/InventoryExit.cs) requires an `ExitReason`. These reasons map directly to specific expense/loss ledger accounts:
- `ExitReason.Shortage` $\rightarrow$ System Ledger Account: `ShortageExpense` (هزینه کسری انبار)
- `ExitReason.Theft` $\rightarrow$ System Ledger Account: `TheftLoss` (زیان سرقت)
- `ExitReason.BarcodeMissing` $\rightarrow$ System Ledger Account: `ShortageExpense` (هزینه کسری انبار)
- `ExitReason.Damage` $\rightarrow$ System Ledger Account: `DamageExpense` (هزینه ضایعات)
- `ExitReason.Gift` $\rightarrow$ System Ledger Account: `GiftExpense` (هزینه هدایا)
- All other reasons default to `OperatingExpenses`.

---

## Inventory Rules & Safety

1. **Precision Rules**: 
   - Floating point types (`float`, `double`) are **never** used. All monetary and weight operations use high-precision `decimal`.
   - Weights are rounded to **3 decimal places** (e.g., in gold weight calculations).
2. **Auditability**: Inventory stock entries ([InventoryStock](file:///d:/source/GoldEx/src/App/Server/GoldEx.Server.Domain/InventoryStockAggregate/InventoryStock.cs)) are write-once. Reversals must be recorded as distinct compensating stock entries (`MarkAsReversalOf`) rather than deleting existing history.
3. **Double-Scan Barcode Prevention**: The `BarcodeReservation` system prevents barcode conflicts. Barcodes transition through `Reserved` $\rightarrow$ `Committed` / `Released` / `Expired`.
