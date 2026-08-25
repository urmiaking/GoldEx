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

        const formatItemFinalPrice = (priceStr) => {
            if (!priceStr) return "";
            const isNegative = String(priceStr).includes("-");
            const num = Math.abs(toNumber(priceStr));
            const formatted = formatNumber(num);
            const unit = String(priceStr).includes("تومان") ? "تومان" : (String(priceStr).includes("ریال") ? "ریال" : "تومان");
            return `<span style="direction: ltr; display: inline-block; unicode-bidi: isolate;">${isNegative ? "-" : ""}${formatted}</span> ${unit}`;
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
              <th style="text-align: left;">مبلغ نهایی</th>
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
                <td class="qi-amount" style="text-align: left; white-space: nowrap;">${formatItemFinalPrice(x.finalPrice)}</td>
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
    },

    printThermalFromPayload: function (payloadJson) {
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

        const formatItemFinalPrice = (priceStr) => {
            if (!priceStr) return "";
            const isNegative = String(priceStr).includes("-");
            const num = Math.abs(toNumber(priceStr));
            const formatted = formatNumber(num);
            const unit = String(priceStr).includes("تومان") ? "تومان" : (String(priceStr).includes("ریال") ? "ریال" : "تومان");
            return `<span style="direction: ltr; display: inline-block; unicode-bidi: isolate;">${isNegative ? "-" : ""}${formatted}</span> ${unit}`;
        };

        const total = items.reduce((acc, x) => acc + toNumber(x.finalPrice), 0);

        const html = `<!doctype html>
<html lang="fa" dir="rtl">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>فیش ${esc(p.invoiceNumber)}</title>
  <style>
    @import url('/fonts/IRANSANS/IRANSANS-font-face.css');
    @page {
      size: 80mm auto;
      margin: 2mm 3mm;
    }
    * { box-sizing: border-box; }
    body {
      font-family: 'IRANSans', 'Tahoma', sans-serif;
      font-size: 11px;
      line-height: 1.5;
      color: #000;
      background: #fff;
      margin: 0;
      padding: 6px 2px;
      width: 76mm;
      max-width: 76mm;
    }
    .pos-header {
      text-align: center;
      border-bottom: 1px dashed #000;
      padding-bottom: 6px;
      margin-bottom: 6px;
    }
    .pos-title { font-size: 14px; font-weight: 900; margin: 0 0 2px 0; }
    .pos-sub { font-size: 10px; color: #333; margin: 1px 0; }
    .pos-info {
      display: flex;
      justify-content: space-between;
      margin: 2px 0;
      font-size: 10px;
    }
    .pos-divider {
      border-bottom: 1px dashed #000;
      margin: 6px 0;
    }
    .pos-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 10px;
      margin: 4px 0;
    }
    .pos-table th {
      border-bottom: 1px solid #000;
      padding: 3px 1px;
      text-align: right;
      font-size: 10px;
    }
    .pos-table td {
      padding: 3px 1px;
      border-bottom: 1px dotted #ccc;
      vertical-align: top;
    }
    .pos-total {
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      font-weight: 900;
      margin: 8px 0 6px 0;
      padding: 4px 0;
      border-top: 1px solid #000;
      border-bottom: 1px solid #000;
    }
    .pos-footer {
      text-align: center;
      font-size: 9px;
      color: #333;
      margin-top: 8px;
      border-top: 1px dashed #000;
      padding-top: 6px;
    }
    .pos-actions {
      display: flex;
      gap: 6px;
      margin-bottom: 8px;
    }
    .pos-btn {
      flex: 1;
      padding: 6px;
      font-size: 11px;
      cursor: pointer;
      font-family: inherit;
    }
    @media print {
      .pos-actions { display: none !important; }
      body { width: 100%; }
    }
  </style>
</head>
<body>
  <div class="pos-actions">
    <button class="pos-btn" onclick="window.close()">بستن</button>
    <button class="pos-btn" style="background:#000; color:#fff; font-weight:bold;" onclick="window.print()">چاپ فیش</button>
  </div>

  <div class="pos-header">
    ${companyLogo ? `<img src="${companyLogo}" alt="لوگو" style="max-height: 36px; max-width: 60px; object-fit: contain; margin-bottom: 2px;" />` : ''}
    <div class="pos-title">${esc(p.companyName || "گالری طلا و جواهر")}</div>
    <div class="pos-sub">${esc(p.companyPhone)}</div>
    <div class="pos-sub">${esc(p.companyAddress)}</div>
  </div>

  <div class="pos-info">
    <span>فاکتور: <b>${esc(p.invoiceNumber)}</b></span>
    <span>تاریخ: <b>${esc(p.dateTime)}</b></span>
  </div>

  <div class="pos-info">
    <span>مشتری: <b>${esc(p.customerName || "مشتری محترم")}</b></span>
    <span>تلفن: <b>${esc(p.customerPhone || "—")}</b></span>
  </div>

  ${p.gramPrice ? `<div class="pos-info"><span>نرخ طلا:</span><span><b>${esc(p.gramPrice)}</b></span></div>` : ''}

  <div class="pos-divider"></div>

  <table class="pos-table">
    <thead>
      <tr>
        <th style="width:12px;">#</th>
        <th>کالا / مشخصات</th>
        <th style="text-align:left;">مبلغ</th>
      </tr>
    </thead>
    <tbody>
      ${items.map((x, i) => `
        <tr>
          <td>${i + 1}</td>
          <td>
            <b>${esc(x.productName || x.productType)}</b><br/>
            <span style="font-size:9px; color:#333;">
              وزن: ${esc(x.weight)} | عیار: ${esc(x.fineness)}
              ${x.wage ? ` | اجرت: ${esc(x.wage)}` : ''}
            </span>
          </td>
          <td style="text-align:left; font-weight:bold; white-space:nowrap;">
            ${formatItemFinalPrice(x.finalPrice)}
          </td>
        </tr>
      `).join("")}
    </tbody>
  </table>

  <div class="pos-total">
    <span>جمع کل پرداختی:</span>
    <span style="direction:ltr;">${formatNumber(total)} تومان</span>
  </div>

  <div class="pos-footer">
    <div>* ارائه فاکتور هنگام تعویض یا فروش الزامی است.</div>
    <div>از حسن اعتماد و خرید شما سپاسگزاریم</div>
  </div>

  <script>
    window.addEventListener('DOMContentLoaded', () => {
      setTimeout(() => window.print(), 300);
    });
  </script>
</body>
</html>`;

        const w = window.open("", "_blank", "popup,width=450,height=600");
        if (!w) return;

        w.document.open();
        w.document.write(html);
        w.document.close();
        w.focus();
    },

    downloadTextAsFile: function (filename, text) {
        const element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', filename);
        element.style.display = 'none';
        document.body.appendChild(element);
        element.click();
        document.body.removeChild(element);
    }
};