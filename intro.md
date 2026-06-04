# Comprehensive Document Introducing the Features and Technical Specifications of the GoldEx Smart System

The GoldEx smart system for jewelry, gold sales, and accounting is designed as a comprehensive, modern solution based on up-to-date technologies to fully cover the needs of the gold and jewelry industry. By leveraging distributed architecture and advanced computational tools, this software simplifies complex processes such as gold accounting, inventory management, customer management, and invoice issuance. This document describes in detail the capabilities of the full GoldEx version as well as its lightweight offline (PWA + LocalStorage) version, GoldEx Mini.

## Part One: Features and Capabilities of GoldEx

### 1. Real-Time Rate and Pricing Management Engine
- **Intelligent Market Monitoring:** Ability to view live and online rates for various types of gold (cast gold, 18K, 24K), precious metals (platinum, palladium), different coins (Emami, Bahar Azadi, half, quarter, gram coins), silver, and common market currencies.
- **Provider Configuration:** Ability to connect to multiple pricing sources and providers and set priority ordering for receiving rates based on the needs of the gallery or bullion dealer.

### 2. Advanced Calculator and Comprehensive Converter System
- **Multi-Purpose Calculation Engine:** A highly advanced computational system for calculating the price of cast gold, jewelry, gemstones, and scrap/used gold using precise industry formulas.
- **Multi-Currency Support:** Price calculation of gold based on multiple international and regional currencies simultaneously.
- **Reverse Converter from Cash to Gold:** A smart tool for converting currency to gold, for example: calculating exactly how many grams of gold can be purchased with 500 million tomans based on the live market rate.
- **Instant Scanning and Pricing:** Ability to scan item barcodes using a smartphone camera or barcode scanner and instantly display rate, weight, and final price based on the live market price.
- **Advanced Search Tool:** A fast search engine for displaying the prices of all items in the showcase or inventory at once.

### 3. Advanced Inventory and Stock Management
- **Supply Chain Management:** Entry and exit of goods and items manually or automatically through purchase and sales invoices in both wholesale and retail modes.
- **Melting Operations:** Ability to register cast gold obtained from melting and accurately calculate gold loss/shrinkage after assay laboratory evaluation.
- **Item Identification:** Ability to print barcodes and dedicated codes for different items, jewelry, and coins for fully automated inventory management.

### 4. Smart Invoicing System and Customer Account Management
- **Multi-Currency Invoices:** Registration of official and unofficial invoices with various currencies in a single system.
- **Visual Customization:** Ability to design custom invoice templates and layouts tailored to the gallery’s brand.
- **Customer Financial Control:** Automatic display of the customer’s previous balance (gold and rial/currency) in the new invoice and full management of receivables and payables.
- **Multiple Settlement Methods:** Ability to register payments and invoice settlements using various types of gold (miscellaneous, cast, broken), cash in different currencies, bank checks, and financial transfers.
- **Expenses and Discounts:** Accurate registration of applied discounts and ancillary costs (such as setting work, plating, and stones).
- **Invoice Item Diversity:** Simultaneous inclusion of gold, jewelry, cast gold, coins, currency, and used gold in a single invoice.

### 5. Automated Accounting and Financial Cashboxes
- **Smart Accounting Without Manual Journals:** Full compliance with standard double-entry gold accounting principles without requiring specialized knowledge or manual journal entry; all transactions, documents, and ledger accounts are generated automatically by the system.
- **Multiple Cashboxes:** Definition and management of different financial cashboxes (rial, foreign currency, gold) to precisely separate receivables, payables, and accounting books.

### 6. Communication, Themes, and Supporting Tools
- **Due Date Alert System:** Receive notifications for overdue invoices and checks, with the ability to automatically send reminder SMS messages to customers.
- **Theme Customization:** Full support for dark and light themes with various colors to suit user preferences.
- **Data Security:** Ability to back up and restore data periodically and securely.
- **Contextual Help:** A dedicated, step-by-step help system for all pages of the software to improve usability.

### 7. Comprehensive Accounting and Management Reports
The software is equipped with a variety of dynamic reports with advanced filtering capabilities:
- General account turnover report and trial balance / general ledger report.
- Customer account balance report and dedicated transaction history for each customer (gold and money statement).
- Comprehensive list of purchase invoices, sales invoices, return invoices, and pro forma invoices.
- Detailed report of receipts, payments, and line-item settlements related to each invoice.
- Report on payment and receivable documents (checks and transfers).
- Item cardex, exact stock of goods, coins, and currency by warehouse.

### 8. Technical Architecture and High-Level Security (Tech Stack)
- **Development Technology:** Built with the powerful MudBlazor framework and Blazor WebAssembly in Auto Render Mode with prerendering capabilities to improve initial load speed and SEO.
- **PWA Support:** Ability to install the software as a Progressive Web App on Windows, Android, and iOS without needing to open a browser.
- **Enterprise Multi-Layer Authentication:** Secure login through strong passwords, authenticator apps, hardware security keys, biometric authentication (passkeys), SMS two-factor authentication (2FA), and direct login with Google accounts.

## Part Two: GoldEx Mini Sales and Calculation Mini-App

GoldEx Mini is an extremely fast and practical mini-application that works completely offline without requiring internet access (only once when installing as PWA and getting the gold and currency price for the first time). In this version, all data is stored locally on the user’s device (LocalStorage), providing high security and privacy. GoldEx Mini includes the calculation features of the main version along with fast sales capabilities.

### Main Features of GoldEx Mini:
- **Cast Gold Price Calculation:** Instant calculation based on the daily gold rate and the gallery’s custom profit percentage.
- **Used Gold Calculation:** Accurate purchase price calculation after deducting workmanship and applying legal industry percentages.
- **Advanced Currency Converter:** Ability to convert various currencies with custom and manual rate input offline.
- **Weight Calculation Based on Budget:** Accurate calculation of the amount of gold that can be purchased with a given budget and current market rate.
- **Fast and Instant Invoice Issuance:** Ability to quickly add multiple items to the cart and print the invoice immediately without delay.
- **Invoice Basket Management:** Ability to view, edit, and delete each item in the invoice basket before finalizing and printing.
- **Invoice History:** Ability to view the full archive of printed invoices on the device with re-print capability.
- **Standard Invoice Printing:** Optimized for standard A5 landscape printing with complete inclusion of daily rate, workmanship, profit, and total invoice amount.
