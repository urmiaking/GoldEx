window.quickInvoice = {
    printFromPayload: function (payloadJson) {
        const parsed = JSON.parse(payloadJson);
        const items = Array.isArray(parsed) ? parsed : [parsed];

        if (!items.length) {
            return;
        }

        const p = items[0];

        let companyLogo = null;
        try {
            const stored = window.localStorage.getItem("QuickInvoiceCompanyInfo");
            if (stored) {
                const storedObj = JSON.parse(stored);
                companyLogo = storedObj.CompanyLogo || storedObj.companyLogo || null;
            }
        } catch (e) {
            // ignore
        }

        const esc = (v) => {
            if (v === null || v === undefined) return "";
            return String(v)
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll('"', "&quot;")
                .replaceAll("'", "&#39;");
        };

        const toNumber = (value) => {
            if (value === null || value === undefined) return 0;
            const s = String(value);
            const normalized = s.replace(/[^0-9.\-]/g, "");
            const n = parseFloat(normalized);
            return Number.isFinite(n) ? n : 0;
        };

        const formatNumber = (n) => {
            try {
                return new Intl.NumberFormat("fa-IR").format(n);
            } catch {
                return String(n);
            }
        };

        const total = items.reduce((acc, x) => acc + toNumber(x.finalPrice), 0);

        const html = `<!doctype html>
<html lang="fa" dir="rtl">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>فاکتور ${esc(p.invoiceNumber)}</title>

  <link rel="stylesheet" href="/assets/css/quick-invoice.css?v=4" />
</head>
<body class="qi-body">
  <div class="qi-sheet">
    <section class="qi-card">
      <header class="qi-header">
        <div class="qi-title" style="display: flex; flex-direction: row; align-items: center; gap: 12px;">
          ${companyLogo ? `<img src="${companyLogo}" alt="لوگو" style="width: 50px; height: 50px; object-fit: contain; border-radius: 6px; flex-shrink: 0;" />` : ''}
          <div style="display: flex; flex-direction: column; gap: 4px;">
            <h1>فاکتور فروش</h1>
            <p class="sub">${esc(p.companyName)} · ${esc(p.companyPhone)}</p>
            <p class="sub">${esc(p.companyAddress)}</p>
          </div>
        </div>

        <div class="qi-meta">
          <div class="qi-pill">شماره: <b>${esc(p.invoiceNumber)}</b></div>
          <div class="sub">تاریخ: <b>${esc(p.dateTime)}</b></div>
        </div>
      </header>

      <div class="qi-content">
        <div class="qi-panel">
          <div class="qi-kv" style="grid-template-columns: auto 1fr auto 1fr auto 1fr;">
            <div class="k">نام مشتری</div><div class="v">${esc(p.customerName)}</div>
            <div class="k">تلفن</div><div class="v">${esc(p.customerPhone)}</div>
            <div class="k">نرخ روز هر گرم</div><div class="v">${esc(p.gramPrice)}</div>
          </div>
        </div>

        <table class="qi-table" aria-label="اقلام">
          <thead>
            <tr>
              <th>#</th>
              <th>کالا</th>
              <th>وزن</th>
              <th>عیار</th>
              <th>اجرت</th>
              <th>مبلغ نهایی</th>
            </tr>
          </thead>
          <tbody>
            ${items.map((x, i) => `
              <tr>
                <td>${i + 1}</td>
                <td>${esc(x.productName || x.productType)}</td>
                <td>${esc(x.weight)}</td>
                <td>${esc(x.fineness)}</td>
                <td>${esc(x.wage ?? "-")} ${x.wageType ? "(" + esc(x.wageType) + ")" : ""}</td>
                <td class="qi-amount">${esc(x.finalPrice)}</td>
              </tr>
            `).join("")}
          </tbody>
        </table>

        <div class="qi-summary">
          <div class="qi-actions">
            <button class="qi-btn" onclick="window.close()">بستن</button>
            <button class="qi-btn primary" onclick="window.print()">چاپ</button>
          </div>

          <div class="qi-total">
            <span class="label">جمع کل</span>
            <span class="value">${formatNumber(total)} تومان</span>
          </div>
        </div>
      </div>

      <footer class="qi-footer">
        <div style="display:flex; flex-direction:column; gap:6px;">
          <div>* اجناس فوق با اجرت مشخص و سود ${esc(p.profitPercent)} درصد و مالیات ارزش افزوده ${esc(p.taxPercent)} درصد از اجرت و سود عرضه شده و در موقع فروش با فاکتور و به نرخ روز خریداری خواهد شد.</div>
          <div>* اجناس فروخته شده بدون علت پس گرفته نمی‌شود.</div>
        </div>
      </footer>
    </section>
  </div>

  <script>
    window.addEventListener('DOMContentLoaded', () => {
      const images = document.getElementsByTagName('img');
      let loaded = 0;
      if (images.length === 0) {
        setTimeout(() => window.print(), 300);
      } else {
        const onImageLoad = () => {
          loaded++;
          if (loaded === images.length) {
            setTimeout(() => window.print(), 300);
          }
        };
        Array.from(images).forEach(img => {
          if (img.complete) {
            onImageLoad();
          } else {
            img.addEventListener('load', onImageLoad);
            img.addEventListener('error', onImageLoad);
          }
        });
      }
    });
  </script>
</body>
</html>`;

        const w = window.open("", "_blank", "popup,width=900,height=650");
        if (!w) return;

        w.document.open();
        w.document.write(html);
        w.document.close();
        w.focus();
    }
};