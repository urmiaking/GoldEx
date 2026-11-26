# Shared read-only enterprise demo (#290)

Demo is disabled by default. No schema migration, separate database, OTP, SMS provider or external CAPTCHA service is needed. The demo login also uses the local challenge after three failures; existing store login is unchanged.
An administrator creates an active store with slug demo and populates only fictitious data (including settings, names, contacts, photos, and opening balances). Never copy production customer data. The store GUID must be explicitly configured; no default-store fallback is allowed for demo sessions.

Configure the existing enterprise host:
- Demo__Enabled=true
- Demo__Host=demo.goldexsoft.ir
- Demo__StoreId=<GUID of the active demo store>
- Demo__DurationDays=7 (1–30)
- Route the demo hostname via HTTPS to the existing application with the correct Host header. Do not CDN-cache authentication responses or API data.

Registration: /Account/Demo. Valid Iranian mobile + locally generated PNG CAPTCHA (session-bound, single-use, 3-minute expiry); repeated numbers are allowed. Unverified mobile is a separate identity claim, never AppUser.PhoneNumber. Secure random credentials are displayed only on the result page. Demo role, store membership and absolute expiry are created in one transaction.

Access: DemoSignInManager and DemoAccessMiddleware validate role, expiry, exact host and sole store membership; the middleware pins StoreContext and allows only explicit DemoRead GET endpoints, selected WASM business pages and logout/assets. The exact public market-price hub transport is allowed (no business methods); tenant SmartTrayHub remains blocked. A stale authenticated demo login or logout clears the old identity cookie and redirects to a fresh login form, rather than permitting a write under a demo principal. Public market icons and active help articles are available; store app icons are pinned to the demo store. GoldExDbContext also refuses tracked writes for demo principals. Never grant Demo a normal business role or add blanket GET access. Keep SSR circuits, shared invoice links, DevExpress POST reporting, integrations, store switching, settings mutations and user management closed unless separately audited.

Demo invoice printing: the authenticated HTML preview and a dedicated GET /invoices/demo-report/{number}/{invoiceType} export are allowed. The PDF uses the store-specific InvoiceReport layout and scoped invoice data. The demo viewer embeds that PDF and offers opening/printing it directly; ordinary users retain the full DevExpress viewer. Shared links and the shared DevExpress document cache remain blocked for demo.

Before enabling publicly: verify signup with local CAPTCHA, login, logout, login again with the same browser session, price negotiation, page help, demo-only reads, forbidden writes (including direct API calls), expiry, ordinary user regression, and browser layout on mobile and desktop. Test against a non-production database. Review read endpoints and uploaded files for store isolation. Seed sufficient demo data manually. Landing CTA should only be deployed once the demo endpoint works.