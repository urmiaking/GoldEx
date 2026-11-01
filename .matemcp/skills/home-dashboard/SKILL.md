# GoldEx Home dashboard architecture and performance
- Index retains original launcher: new invoice, smart tray, price board, customer transfers and inventory. MarketPriceDeck remains on desktop and PriceBoard on phones. Never remove these during performance refactoring.
- HomeOperations (tab=0) loads only top unpaid invoices and presents the work queue, direct edit links, checks, customers and reports.
- Statistics (tab=5) is lazy. HomeStatistics uses SVG daily purchase/sales bars, HomeInsights loads four KPI cards only when selected, and HomeCategorySales fetches month category details only on expand. Preserve legacy tab ids 1-4.
- Normal font weight 400, medium 500; avoid heavy bold on headers and KPI.
- DashboardRepository uses SQL aggregate projections instead of loading entire Invoice entities for sales, unpaid and trend; balances summarize per customer/price-unit in SQL. Financial computations retain decimal and original domain semantics.
- Honor StoreId tenant filters; avoid fake chart values and timer-induced renders. Benchmark and cross-check values on real data before claiming quantified performance gains.
- Build Release if the existing Debug server holds DLLs. Run focused checks and inspect Git diff. No push or deploy without explicit instruction.
- Daily SVG chart groups are interactive with mouse hover, click/tap, keyboard focus and Enter/Space; hover previews the date, purchase/sales weight and signed difference, while a click pins/unpins the details. Tooltip stays inside the 900px scrollable chart canvas and switches sides to avoid horizontal clipping. These interactions consume existing trend DTOs; no extra API requests.
- HomeCategorySales keeps its expanded body mounted after loading. Grid-template-rows 0fr/1fr and visibility/icon rotation smoothly open/close the panel. Honor prefers-reduced-motion and retain _loaded to avoid refetching.
