/* ==========================================================================
   GoldEx Vitrine - Interactive Helpers, Image Gallery & Multi-Platform Share
   ========================================================================== */

window.goldexVitrine = {
  _currentStoryBlob: null,
  _currentShareData: null,

  changeMainImage: function (imageUrl, thumbElement) {
    var mainImg = document.getElementById("vitrineMainImage");
    if (mainImg) {
      mainImg.style.opacity = "0.3";
      setTimeout(function () {
        mainImg.src = imageUrl;
        mainImg.style.opacity = "1";
      }, 150);
    }
    var thumbs = document.querySelectorAll(".vitrine-thumb");
    thumbs.forEach(function (t) {
      t.classList.remove("active");
    });
    if (thumbElement) {
      thumbElement.classList.add("active");
    }
  },

  copyShareLink: function (customUrl) {
    var url = customUrl || window.location.href;
    if (navigator.clipboard) {
      navigator.clipboard.writeText(url).then(function () {
        window.goldexVitrine.showToast("لینک محصول کپی شد.");
      }).catch(function() {
        window.goldexVitrine.fallbackCopy(url);
      });
    } else {
      window.goldexVitrine.fallbackCopy(url);
    }
  },

  fallbackCopy: function (url) {
    var temp = document.createElement("input");
    document.body.appendChild(temp);
    temp.value = url;
    temp.select();
    document.execCommand("copy");
    document.body.removeChild(temp);
    window.goldexVitrine.showToast("لینک محصول کپی شد.");
  },

  showToast: function (msg) {
    var existing = document.getElementById("vitrineToast");
    if (existing) existing.remove();

    var toast = document.createElement("div");
    toast.id = "vitrineToast";
    toast.className = "vitrine-toast-notification";
    toast.innerText = msg;

    document.body.appendChild(toast);
    setTimeout(function () {
      toast.style.opacity = "0";
      toast.style.transform = "translate(-50%, 20px)";
      setTimeout(function () { toast.remove(); }, 400);
    }, 2500);
  },

  removeSplash: function() {
    if (window.GoldEx && typeof window.GoldEx.removeSplash === "function") {
      window.GoldEx.removeSplash();
    } else {
      var el = document.getElementById('app-loading');
      if (el) {
        el.classList.add('fade-out');
        setTimeout(function () { el.remove(); }, 300);
      }
    }
  },

  setFilterDrawerOpen: function (isOpen) {
    if (isOpen) {
      document.body.classList.add("vitrine-filter-drawer-open");
    } else {
      document.body.classList.remove("vitrine-filter-drawer-open");
    }
  },

  // --------------------------------------------------------------------------
  // 9:16 (1080x1920) Story Card Generator on HTML5 Canvas
  // --------------------------------------------------------------------------
  generateStoryCardBlob: function (data) {
    return new Promise(function (resolve, reject) {
      try {
        var canvas = document.createElement("canvas");
        canvas.width = 1080;
        canvas.height = 1920;
        var ctx = canvas.getContext("2d");
        if (!ctx) return reject("Canvas 2D context not available");

        // 1. Background Gradient (Dark Luxury Obsidian)
        var bgGradient = ctx.createLinearGradient(0, 0, 0, 1920);
        bgGradient.addColorStop(0, "#08090d");
        bgGradient.addColorStop(0.3, "#0f131a");
        bgGradient.addColorStop(0.7, "#151922");
        bgGradient.addColorStop(1, "#08090d");
        ctx.fillStyle = bgGradient;
        ctx.fillRect(0, 0, 1080, 1920);

        // Radial Gold Ambient Glow
        var radialGlow = ctx.createRadialGradient(540, 720, 60, 540, 720, 550);
        radialGlow.addColorStop(0, "rgba(218, 165, 32, 0.18)");
        radialGlow.addColorStop(0.7, "rgba(218, 165, 32, 0.04)");
        radialGlow.addColorStop(1, "rgba(0, 0, 0, 0)");
        ctx.fillStyle = radialGlow;
        ctx.fillRect(0, 0, 1080, 1920);

        // 2. Elegant Border Frame
        ctx.strokeStyle = "rgba(218, 165, 32, 0.3)";
        ctx.lineWidth = 3;
        ctx.strokeRect(36, 36, 1008, 1848);

        ctx.strokeStyle = "rgba(218, 165, 32, 0.85)";
        ctx.lineWidth = 1;
        ctx.strokeRect(46, 46, 988, 1828);

        // Ornate Corners
        function drawCorner(x, y, dx, dy) {
          ctx.fillStyle = "#d4af37";
          ctx.beginPath();
          ctx.arc(x, y, 5, 0, Math.PI * 2);
          ctx.fill();

          ctx.strokeStyle = "#daa520";
          ctx.lineWidth = 3.5;
          ctx.beginPath();
          ctx.moveTo(x, y + dy * 24);
          ctx.lineTo(x, y);
          ctx.lineTo(x + dx * 24, y);
          ctx.stroke();
        }
        drawCorner(46, 46, 1, 1);
        drawCorner(1034, 46, -1, 1);
        drawCorner(46, 1874, 1, -1);
        drawCorner(1034, 1874, -1, -1);

        // 3. Store Header
        ctx.textAlign = "center";
        ctx.direction = "rtl";

        ctx.font = "bold 44px 'IRANSans', system-ui, -apple-system, sans-serif";
        ctx.fillStyle = "#e0b64a";
        var storeTitle = data.storeName || "گالری طلا و جواهر";
        ctx.fillText(storeTitle, 540, 125);

        ctx.font = "24px 'IRANSans', system-ui, -apple-system, sans-serif";
        ctx.fillStyle = "#9ca3af";
        ctx.fillText("ویترین رسمی آنلاین طلا و جواهرات", 540, 170);

        // Header Gold Divider
        var lineGrad = ctx.createLinearGradient(300, 200, 780, 200);
        lineGrad.addColorStop(0, "rgba(218, 165, 32, 0)");
        lineGrad.addColorStop(0.5, "rgba(218, 165, 32, 0.7)");
        lineGrad.addColorStop(1, "rgba(218, 165, 32, 0)");
        ctx.strokeStyle = lineGrad;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(300, 200);
        ctx.lineTo(780, 200);
        ctx.stroke();

        // Helper for rounded rectangles
        function drawRoundRect(c, x, y, w, h, r) {
          if (w < 2 * r) r = w / 2;
          if (h < 2 * r) r = h / 2;
          c.beginPath();
          c.moveTo(x + r, y);
          c.arcTo(x + w, y, x + w, y + h, r);
          c.arcTo(x + w, y + h, x, y + h, r);
          c.arcTo(x, y + h, x, y, r);
          c.arcTo(x, y, x + w, y, r);
          c.closePath();
        }

        function drawPill(c, x, y, w, h, bg, textColor, text) {
          var r = h / 2;
          c.save();
          drawRoundRect(c, x, y, w, h, r);
          c.fillStyle = bg;
          c.fill();
          c.strokeStyle = "rgba(218, 165, 32, 0.4)";
          c.lineWidth = 1.5;
          c.stroke();
          c.font = "bold 22px 'IRANSans', system-ui, -apple-system, sans-serif";
          c.fillStyle = textColor;
          c.textAlign = "center";
          c.direction = "rtl";
          c.fillText(text, x + w / 2, y + h / 2 + 7);
          c.restore();
        }

        // 4. Product Image Box
        var cardX = 90;
        var cardY = 240;
        var cardW = 900;
        var cardH = 900;
        var cardR = 28;

        // Container Round Rect
        ctx.save();
        drawRoundRect(ctx, cardX, cardY, cardW, cardH, cardR);
        ctx.fillStyle = "#11141c";
        ctx.fill();
        ctx.strokeStyle = "rgba(218, 165, 32, 0.4)";
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.clip();

        var img = new Image();
        img.crossOrigin = "anonymous";

        function finalizeCanvas() {
          ctx.restore();

          // Badges on Image
          drawPill(ctx, 115, 265, 170, 52, "#181b24", "#d4af37", "عیار " + (data.fineness || "۷۵۰"));
          if (data.weight) {
            var weightCaption = data.isPartiallySold ? "باقی‌مانده: " : "وزن: ";
            drawPill(ctx, 745, 265, 220, 52, "#181b24", "#f3f4f6", weightCaption + data.weight + " گرم");
          }

          // 5. Product Details Section
          ctx.direction = "rtl";
          ctx.textAlign = "center";

          // Title
          ctx.font = "bold 52px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#ffffff";
          var pName = data.productName || "طلا و جواهر فاخر";
          if (pName.length > 32) pName = pName.substring(0, 30) + "...";
          ctx.fillText(pName, 540, 1220);

          // Sub-specs
          var specsText = (data.categoryTitle ? data.categoryTitle + "  •  " : "") + "طلای ۱۸ عیار استاندارد";
          ctx.font = "26px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#d1d5db";
          ctx.fillText(specsText, 540, 1275);

          // 6. Price Highlight Box
          var priceBoxY = 1320;
          var priceBoxW = 760;
          var priceBoxH = 150;
          var priceBoxX = (1080 - priceBoxW) / 2;
          var pR = 22;

          ctx.save();
          drawRoundRect(ctx, priceBoxX, priceBoxY, priceBoxW, priceBoxH, pR);
          var priceGrad = ctx.createLinearGradient(priceBoxX, priceBoxY, priceBoxX + priceBoxW, priceBoxY + priceBoxH);
          priceGrad.addColorStop(0, "rgba(218, 165, 32, 0.2)");
          priceGrad.addColorStop(1, "rgba(218, 165, 32, 0.06)");
          ctx.fillStyle = priceGrad;
          ctx.fill();
          ctx.strokeStyle = "rgba(218, 165, 32, 0.6)";
          ctx.lineWidth = 2;
          ctx.stroke();
          ctx.restore();

          // Price Label
          ctx.font = "22px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#9ca3af";
          ctx.fillText(data.isPartiallySold ? "قیمت بخش باقی‌مانده بر اساس مظنه روز" : "محاسبه زنده بر اساس مظنه روز طلا", 540, priceBoxY + 45);

          // Price Amount
          ctx.font = "900 48px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#e0b64a";
          var priceStr = data.priceFormatted ? (data.priceFormatted + " تومان") : "استعلام تماس";
          ctx.fillText(priceStr, 540, priceBoxY + 110);

          // 7. Footer Call-to-Action Card
          var ctaY = 1520;
          var ctaW = 860;
          var ctaH = 190;
          var ctaX = (1080 - ctaW) / 2;
          var cR = 20;

          ctx.save();
          drawRoundRect(ctx, ctaX, ctaY, ctaW, ctaH, cR);
          ctx.fillStyle = "rgba(16, 20, 28, 0.9)";
          ctx.fill();
          ctx.strokeStyle = "rgba(255, 255, 255, 0.12)";
          ctx.lineWidth = 1.5;
          ctx.stroke();
          ctx.restore();

          ctx.font = "bold 26px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#f3f4f6";
          ctx.fillText("🔗 مشاهده آنلاین و استعلام در ویترین گالری", 540, ctaY + 60);

          ctx.font = "22px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#9ca3af";
          var codeText = data.barcode ? ("کد بارکد محصول: " + data.barcode) : "";
          ctx.fillText(codeText, 540, ctaY + 105);

          ctx.font = "bold 22px 'IRANSans', system-ui, -apple-system, sans-serif";
          ctx.fillStyle = "#d4af37";
          ctx.fillText(window.location.host, 540, ctaY + 150);

          // Convert to Blob
          canvas.toBlob(function (blob) {
            if (blob) resolve(blob);
            else reject("Could not generate image blob");
          }, "image/png", 0.95);
        }

        img.onload = function () {
          var iw = img.naturalWidth || img.width;
          var ih = img.naturalHeight || img.height;
          var scale = Math.min((cardW - 60) / iw, (cardH - 60) / ih);
          var dw = iw * scale;
          var dh = ih * scale;
          var dx = cardX + (cardW - dw) / 2;
          var dy = cardY + (cardH - dh) / 2;

          ctx.drawImage(img, dx, dy, dw, dh);
          finalizeCanvas();
        };

        img.onerror = function () {
          // Fallback placeholder if image load failed
          ctx.fillStyle = "#e0b64a";
          ctx.font = "bold 70px sans-serif";
          ctx.fillText("💎", cardX + cardW / 2, cardY + cardH / 2 + 25);
          finalizeCanvas();
        };

        img.src = data.imageUrl;
      } catch (err) {
        reject(err);
      }
    });
  },

  // --------------------------------------------------------------------------
  // Share & Story Action
  // --------------------------------------------------------------------------
  shareProduct: function (data) {
    window.goldexVitrine._currentShareData = data;
    window.goldexVitrine.openShareModal(data);
  },

  // Direct Download of Story Card Image
  downloadStoryCard: async function (data) {
    data = data || window.goldexVitrine._currentShareData;
    if (!data) return;

    window.goldexVitrine.showToast("در حال ساخت عکس باکیفیت استوری...");
    try {
      var blob = window.goldexVitrine._currentStoryBlob;
      if (!blob) {
        blob = await window.goldexVitrine.generateStoryCardBlob(data);
        window.goldexVitrine._currentStoryBlob = blob;
      }

      var blobUrl = URL.createObjectURL(blob);
      var a = document.createElement("a");
      a.href = blobUrl;
      a.download = "story-" + (data.barcode || "product") + ".png";
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      setTimeout(function () { URL.revokeObjectURL(blobUrl); }, 1000);
      window.goldexVitrine.showToast("عکس استوری دانلود شد.");
    } catch (e) {
      console.error(e);
      window.goldexVitrine.showToast("خطا در دانلود عکس استوری");
    }
  },

  openShareModal: function (data) {
    data = data || window.goldexVitrine._currentShareData;
    if (!data) return;

    var existing = document.getElementById("vitrineShareModal");
    if (existing) existing.remove();

    var productUrl = data.url;
    if (!productUrl || productUrl.indexOf("://p/") !== -1 || productUrl.startsWith("//p/")) {
      productUrl = window.location.href;
    }

    var encodedUrl = encodeURIComponent(productUrl);
    var shareMsg = (data.productName ? data.productName + "\n" : "") +
                   (data.statusText ? data.statusText + "\n" : "") +
                   (data.priceFormatted ? (data.isPartiallySold ? "قیمت بخش باقی‌مانده: " : "قیمت روز: ") + data.priceFormatted + " تومان\n" : "") +
                   (data.weight ? (data.isPartiallySold ? "وزن باقی‌مانده: " : "وزن: ") + data.weight + " گرم - عیار " + (data.fineness || "750") + "\n" : "") +
                   "مشاهده در ویترین:";
    var encodedText = encodeURIComponent(shareMsg);

    var telegramUrl = "https://t.me/share/url?url=" + encodedUrl + "&text=" + encodedText;
    var whatsappUrl = "https://api.whatsapp.com/send?text=" + encodedText + "%20" + encodedUrl;
    var baleUrl = "https://ble.ir/share/url?url=" + encodedUrl + "&text=" + encodedText;
    var eitaaUrl = "https://eitaa.com/share/url?url=" + encodedUrl + "&text=" + encodedText;

    var modalHtml = `
      <div id="vitrineShareModal" class="vitrine-share-modal-overlay" onclick="if(event.target === this) window.goldexVitrine.closeShareModal();">
        <div class="vitrine-share-modal-content animate-slide-up">
          <div class="vitrine-share-modal-header">
            <div class="d-flex align-items-center gap-2">
              <i class="fa-solid fa-share-nodes" style="color: var(--accent-color, #d5b267); font-size: 1.25rem;"></i>
              <h3 style="margin: 0; font-size: 1.15rem; font-weight: 800; color: #fff; font-family: inherit;">اشتراک‌گذاری کالا</h3>
            </div>
            <button type="button" class="vitrine-share-close-btn" onclick="window.goldexVitrine.closeShareModal()">&times;</button>
          </div>

          <!-- Story Card Highlight -->
          <div class="vitrine-story-card-banner" onclick="window.goldexVitrine.downloadStoryCard()">
            <div class="d-flex align-items-center gap-3">
              <div class="vitrine-story-icon-box">
                <i class="fa-brands fa-instagram" style="font-size: 1.8rem;"></i>
              </div>
              <div style="flex: 1; text-align: right;">
                <div style="font-weight: 800; font-size: 0.98rem; color: #fff; font-family: inherit;">دانلود کارت آماده استوری اینستاگرام</div>
                <div style="font-size: 0.8rem; color: #9ca3af; font-family: inherit; margin-top: 2px;">طراحی شیک عمودی (۹:۱۶) همراه با عکس و مشخصات کامل</div>
              </div>
              <i class="fa-solid fa-download" style="color: var(--accent-color, #d5b267); font-size: 1.1rem;"></i>
            </div>
          </div>

          <!-- Apps Grid -->
          <div class="vitrine-share-grid">
            <a href="${telegramUrl}" target="_blank" class="vitrine-share-app-item share-telegram">
              <div class="share-app-icon"><i class="fa-brands fa-telegram"></i></div>
              <span>تلگرام</span>
            </a>
            <a href="${whatsappUrl}" target="_blank" class="vitrine-share-app-item share-whatsapp">
              <div class="share-app-icon"><i class="fa-brands fa-whatsapp"></i></div>
              <span>واتساپ</span>
            </a>
            <a href="${baleUrl}" target="_blank" class="vitrine-share-app-item share-bale">
              <div class="share-app-icon"><i class="fa-solid fa-comment"></i></div>
              <span>بله</span>
            </a>
            <a href="${eitaaUrl}" target="_blank" class="vitrine-share-app-item share-eitaa">
              <div class="share-app-icon"><i class="fa-solid fa-paper-plane"></i></div>
              <span>ایتا</span>
            </a>
          </div>

          <!-- Copy Link Section -->
          <div class="vitrine-share-link-box">
            <input type="text" readonly class="vitrine-share-link-input" />
            <button type="button" class="vitrine-share-copy-btn">
              <i class="fa-solid fa-copy"></i>
              <span>کپی لینک</span>
            </button>
          </div>
        </div>
      </div>
    `;

    document.body.insertAdjacentHTML("beforeend", modalHtml);
    var modal = document.getElementById("vitrineShareModal");
    modal.querySelector(".vitrine-share-link-input").value = productUrl;
    modal.querySelector(".vitrine-share-copy-btn").addEventListener("click", function () {
      window.goldexVitrine.copyShareLink(productUrl);
    });
  },

  nativeShare: async function () {
    var data = window.goldexVitrine._currentShareData;
    if (!data) return;
    var productUrl = data.url || window.location.href;
    var sharePayload = {
      title: data.productName || "طلا و جواهر",
      text: (data.productName ? data.productName + " - " : "") + (data.statusText ? data.statusText + " - " : "") + (data.priceFormatted ? ((data.isPartiallySold ? "قیمت بخش باقی‌مانده: " : "قیمت: ") + data.priceFormatted + " تومان - ") : "") + "مشاهده در ویترین: " + productUrl,
      url: productUrl
    };
    if (navigator.share) {
      try {
        await navigator.share(sharePayload);
      } catch (e) {}
    }
  },

  closeShareModal: function () {
    var modal = document.getElementById("vitrineShareModal");
    if (modal) {
      modal.classList.add("fade-out");
      setTimeout(function () { modal.remove(); }, 250);
    }
  }
};

document.addEventListener('click', (event) => {
  if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

  const share = event.target.closest('[data-vitrine-share]');
  if (share) {
    window.goldexVitrine.shareProduct(JSON.parse(share.dataset.vitrineShare));
  }
});

// Close the share dialog or the catalog filter drawer on Escape.
document.addEventListener("keydown", function (e) {
  if (e.key !== "Escape" || e.isComposing || e.defaultPrevented) return;

  if (document.getElementById('vitrineShareModal')) {
    window.goldexVitrine.closeShareModal();
  } else if (document.body.classList.contains("vitrine-filter-drawer-open")) {
    var closeBtn = document.querySelector(".catalog-drawer-close");
    if (closeBtn) closeBtn.click();
  }
});
function initializeVitrineSearch(panel) {
  const toggle = document.querySelector('[data-vitrine-search-toggle]');
  const form = panel.querySelector('form[role="search"]');
  const input = form.elements.q;
  const results = panel.querySelector('[data-vitrine-search-results]');
  const status = panel.querySelector('[data-vitrine-search-status]');
  const prices = new Intl.NumberFormat(document.documentElement.lang || 'fa', { maximumFractionDigits: 0 });
  let data;
  let request;
  let timer;

  function element(tag, className, content) {
    const node = document.createElement(tag);
    node.className = className;
    if (content != null) node.textContent = content;
    return node;
  }

  function showStatus(message) {
    status.textContent = message;
    status.hidden = !message;
  }

  function render(term, products, categories) {
    const matches = (value) => value?.toLocaleLowerCase().includes(term);
    const matchingCategories = categories.filter(category => matches(category.title)).slice(0, 3);
    const matchingProducts = products.filter(product => matches(product.name) || matches(product.barcode) || matches(product.categoryTitle));
    const catalogUrl = new URL(form.action);
    showStatus(matchingCategories.length || matchingProducts.length ? '' : 'محصولی پیدا نشد.');
    const fragment = document.createDocumentFragment();

    if (matchingCategories.length) {
      const section = element('section', 'vitrine-dropdown-section');
      section.append(element('h3', 'vitrine-dropdown-section-title', 'دسته‌بندی‌های مرتبط'));
      const list = element('div', 'vitrine-dropdown-categories');
      matchingCategories.forEach(category => {
        const link = element('a', 'vitrine-dropdown-cat-pill', category.title);
        const url = new URL(catalogUrl);
        url.searchParams.set('categoryId', category.id);
        link.href = url;
        link.append(element('span', 'vitrine-dropdown-cat-count', `${category.productCount} کالا`));
        list.append(link);
      });
      section.append(list);
      fragment.append(section);
    }

    if (matchingProducts.length) {
      const section = element('section', 'vitrine-dropdown-section');
      section.append(element('h3', 'vitrine-dropdown-section-title', `محصولات (${matchingProducts.length})`));
      const list = element('div', 'vitrine-dropdown-products-list');
      matchingProducts.slice(0, 5).forEach(product => {
        const link = element('a', 'vitrine-dropdown-product-item');
        link.href = panel.dataset.productBaseUrl + encodeURIComponent(product.barcode);
        const imageFrame = element('span', 'vitrine-dropdown-product-img');
        if (product.mainImageUrl) {
          const image = document.createElement('img');
          image.src = product.mainImageUrl;
          image.alt = '';
          image.loading = 'lazy';
          imageFrame.append(image);
        } else {
          imageFrame.append(element('i', 'fa-solid fa-gem'));
        }

        const details = element('span', 'vitrine-dropdown-product-meta');
        details.append(element('span', 'vitrine-dropdown-product-name', product.name));
        const weight = `${product.weight} گرم`;
        details.append(element('span', 'vitrine-dropdown-product-sub', [
          product.categoryTitle || 'طلا',
          product.isPartiallySold ? `باقی‌مانده: ${weight}` : weight,
          `عیار ${product.fineness || 750}`
        ].join(' • ')));
        if (!product.isAvailable || product.isPartiallySold) {
          details.append(element('span', 'vitrine-dropdown-product-sub', product.isAvailable ? 'بخشی فروخته شده' : 'فروخته شد'));
        }
        const price = element('span', 'vitrine-dropdown-product-price');
        price.append(element('span', 'vitrine-dropdown-price-val', prices.format(product.estimatedPrice)),
          element('span', 'vitrine-dropdown-price-unit', ' تومان'));
        link.append(imageFrame, details, price);
        list.append(link);
      });
      section.append(list);
      const all = element('a', 'vitrine-dropdown-view-all-btn', 'مشاهده همهٔ نتایج در کاتالوگ');
      catalogUrl.searchParams.set('q', input.value.trim());
      all.href = catalogUrl;
      section.append(all);
      fragment.append(section);
    }
    results.replaceChildren(fragment);
  }

  async function search() {
    const term = input.value.trim().toLocaleLowerCase();
    if (!term || panel.inert) return;

    const controller = new AbortController();
    request = controller;
    try {
      if (!data) {
        showStatus('در حال دریافت محصولات…');
        const load = async (url) => {
          const response = await fetch(url, { signal: controller.signal });
          if (!response.ok) throw new Error('Vitrine search request failed');
          return response.json();
        };
        const [products, categories] = await Promise.all([load(panel.dataset.productsUrl), load(panel.dataset.categoriesUrl)]);
        products.sort((first, second) => Number(second.isAvailable) - Number(first.isAvailable) ||
          second.createdAt.localeCompare(first.createdAt));
        data = [products, categories];
      }
      if (!controller.signal.aborted) render(term, ...data);
    } catch {
      if (!controller.signal.aborted) showStatus('دریافت نتیجه‌ها ممکن نشد. دوباره جستجو کنید.');
    } finally {
      if (request === controller) request = undefined;
    }
  }

  input.addEventListener('input', (event) => {
    if (event.isComposing) return;
    clearTimeout(timer);
    request?.abort();
    results.replaceChildren();
    showStatus('');
    if (input.value.trim()) timer = setTimeout(search, data ? 0 : 180);
  });
  function setOpen(open) {
    toggle.setAttribute('aria-expanded', String(open));
    if (!open && panel.contains(document.activeElement)) toggle.focus({ preventScroll: true });
    panel.inert = !open;

    if (open) {
      input.focus({ preventScroll: true });
      if (input.value.trim()) search();
      return;
    }
    clearTimeout(timer);
    request?.abort();
  }

  toggle.addEventListener('click', (event) => {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    event.preventDefault();
    setOpen(panel.inert);
  });
  panel.querySelector('[data-vitrine-search-close]').addEventListener('click', () => setOpen(false));
  panel.closest('header').addEventListener('keydown', (event) => {
    if (event.key !== 'Escape' || event.isComposing || panel.inert) return;
    event.preventDefault();
    setOpen(false);
  });
}

const vitrineSearch = document.getElementById('vitrine-search-panel');
if (vitrineSearch) initializeVitrineSearch(vitrineSearch);

function initializeVitrineCatalog() {
  let catalog = document.querySelector('[data-vitrine-catalog]');
  if (!catalog) return;

  const getDrawer = () => catalog.querySelector('.catalog-filter-drawer');
  const getOpener = () => catalog.querySelector('[data-vitrine-filter-open]');
  let request;
  let searchTimer;

  function setDrawer(open, moveFocus = true) {
    const drawer = getDrawer();
    drawer.classList.toggle('open', open);
    drawer.inert = !open;
    drawer.setAttribute('aria-hidden', String(!open));
    getOpener().setAttribute('aria-expanded', String(open));
    catalog.querySelector('.catalog-drawer-backdrop').classList.toggle('open', open);
    window.goldexVitrine.setFilterDrawerOpen(open);
    if (moveFocus) {
      (open ? drawer.querySelector('.catalog-drawer-close') : getOpener()).focus({ preventScroll: true });
    }
  }

  async function updateCatalog(url, searchForm, loadMore = false) {
    clearTimeout(searchTimer);
    request?.abort();
    const controller = new AbortController();
    request = controller;
    catalog.setAttribute('aria-busy', 'true');

    try {
      const response = await fetch(url, { signal: controller.signal });
      if (!response.ok || response.redirected) throw new Error('Could not load catalog');
      const documentFragment = new DOMParser().parseFromString(await response.text(), 'text/html');
      const updated = documentFragment.querySelector('[data-vitrine-catalog]');
      if (!updated) throw new Error('Catalog missing from response');
      if (controller.signal.aborted) return;

      const drawer = getDrawer();
      const drawerOpen = drawer.classList.contains('open');
      const drawerScroll = drawer.querySelector('.catalog-drawer-body').scrollTop;
      const openedFacets = new Set(Array.from(catalog.querySelectorAll('details[open]'), details => details.dataset.vitrineFacet));
      const focused = document.activeElement;
      const activeSearch = searchForm?.contains(focused);
      const selection = activeSearch ? [focused.selectionStart, focused.selectionEnd] : null;
      const previousCardCount = loadMore ? catalog.querySelectorAll('.product-card').length : 0;

      catalog.replaceWith(updated);
      catalog = updated;
      updated.querySelectorAll('details').forEach(details => {
        if (openedFacets.has(details.dataset.vitrineFacet)) details.open = true;
      });
      setDrawer(drawerOpen, false);
      getDrawer().querySelector('.catalog-drawer-body').scrollTop = drawerScroll;
      if (activeSearch) {
        const area = drawerOpen ? getDrawer() : catalog.querySelector('.shop-sidebar');
        const input = area.querySelector('input[name="q"]');
        input.focus({ preventScroll: true });
        input.setSelectionRange(...selection);
      } else if (drawerOpen) {
        getDrawer().querySelector('.catalog-drawer-close').focus({ preventScroll: true });
      } else if (loadMore) {
        catalog.querySelectorAll('.product-card')[previousCardCount]?.focus({ preventScroll: true });
      } else if (focused?.matches('[data-vitrine-catalog-link]')) {
        const matchingLink = Array.from(catalog.querySelectorAll('[data-vitrine-catalog-link]'))
          .find(link => link.href === focused.href && link.getClientRects().length);
        (matchingLink || catalog.querySelector('.shop-products')).focus({ preventScroll: true });
      }

      if (url.href !== location.href) {
        if (searchForm && history.state?.vitrineSearch) history.replaceState(history.state, '', url);
        else history.pushState({ vitrineSearch: Boolean(searchForm) }, '', url);
      }
    } catch {
      if (!controller.signal.aborted) location.assign(url);
    } finally {
      if (request === controller) {
        request = undefined;
        catalog.removeAttribute('aria-busy');
      }
    }
  }

  function searchCatalog(form) {
    const url = new URL(form.action);
    url.search = new URLSearchParams(new FormData(form));
    if (!url.searchParams.get('q').trim()) url.searchParams.delete('q');
    updateCatalog(url, form);
  }

  document.addEventListener('click', (event) => {
    if (!catalog.contains(event.target) || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    if (event.target.closest('[data-vitrine-filter-open]')) {
      setDrawer(true);
    } else if (event.target.closest('[data-vitrine-filter-close]')) {
      setDrawer(false);
    } else {
      const link = event.target.closest('[data-vitrine-catalog-link]');
      if (!link) return;
      event.preventDefault();
      updateCatalog(new URL(link.href), null, link.classList.contains('btn-load-more'));
    }
  });

  document.addEventListener('submit', (event) => {
    if (!event.target.matches('[data-vitrine-catalog-search]')) return;
    event.preventDefault();
    searchCatalog(event.target);
  });
  document.addEventListener('input', (event) => {
    const form = event.target.closest('[data-vitrine-catalog-search]');
    if (!form || event.isComposing) return;
    clearTimeout(searchTimer);
    request?.abort();
    searchTimer = setTimeout(() => searchCatalog(form), 260);
  });
  matchMedia('(min-width: 992px)').addEventListener('change', (event) => {
    if (event.matches && getDrawer().classList.contains('open')) setDrawer(false, false);
  });
  window.addEventListener('popstate', () => location.reload());
}

initializeVitrineCatalog();

customElements.define('vitrine-campaign', class extends HTMLElement {
  connectedCallback() {
    queueMicrotask(() => {
      if (this.isConnected && !this.stop) this.initialize();
    });
  }

  initialize() {
    // The prerendered slide also works before Blazor or when JavaScript is unavailable.
    const track = this.querySelector('[data-campaign-slides]');
    if (!track) return;

    const slides = Array.from(track.querySelectorAll('[data-campaign-slide]'));
    const controls = this.querySelector('[data-campaign-controls]');
    const indicators = Array.from(controls.querySelectorAll('[data-campaign-indicator]'));
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    const forward = getComputedStyle(track).direction === 'rtl' ? 1 : -1;
    const listeners = new AbortController();
    const { signal } = listeners;
    let current = 0;
    let manuallySelected = false;
    let hovering = false;
    let timer;
    let gesture;
    let animations = [];
    let ignoreClickUntil = 0;

    const restart = () => {
      clearTimeout(timer);
      const autoplay = !manuallySelected && !reducedMotion.matches;
      track.setAttribute('aria-live', autoplay ? 'off' : 'polite');
      if (autoplay && !hovering && !gesture && !document.hidden && !this.contains(document.activeElement)) {
        timer = setTimeout(() => show(current + 1, forward), 6500);
      }
    };

    const settle = () => {
      animations.forEach(animation => animation.cancel());
      animations = [];
      slides.forEach((slide, i) => {
        slide.hidden = i !== current;
        slide.inert = i !== current;
      });
    };

    const show = (index, direction, manual = false) => {
      manuallySelected ||= manual;
      restart();
      const next = (index + slides.length) % slides.length;
      if (next === current) return;

      settle();
      const outgoing = slides[current];
      current = next;
      const incoming = slides[current];
      indicators.forEach((indicator, i) => {
        if (i === current) indicators[i].setAttribute('aria-current', 'true');
        else indicators[i].removeAttribute('aria-current');
      });

      if (reducedMotion.matches) {
        settle();
        return;
      }

      outgoing.inert = true;
      incoming.inert = false;
      incoming.hidden = false;
      const timing = { duration: 350, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'both' };
      const outgoingMotion = outgoing.animate({ transform: ['translateX(0)', `translateX(${direction * 100}%)`] }, timing);
      const incomingMotion = incoming.animate({ transform: [`translateX(${-direction * 100}%)`, 'translateX(0)'] }, timing);
      animations = [outgoingMotion, incomingMotion];
      incomingMotion.onfinish = () => {
        if (animations.includes(incomingMotion)) settle();
      };
    };

    indicators.forEach((button, index) => {
      button.addEventListener('click', () => show(index, Math.sign(index - current) * forward, true), { signal });
    }, { signal });
    this.addEventListener('pointerenter', (event) => {
      if (event.pointerType !== 'mouse') return;
      hovering = true;
      restart();
    }, { signal });
    this.addEventListener('pointerleave', (event) => {
      if (event.pointerType !== 'mouse') return;
      hovering = false;
      restart();
    }, { signal });
    this.addEventListener('focusin', restart, { signal });
    this.addEventListener('focusout', () => queueMicrotask(() => {
      if (!signal.aborted) restart();
    }), { signal });
    document.addEventListener('visibilitychange', restart, { signal });
    reducedMotion.addEventListener('change', () => {
      if (reducedMotion.matches) settle();
      restart();
    }, { signal });

    track.addEventListener('pointerdown', (event) => {
      ignoreClickUntil = 0;
      if (event.pointerType === 'touch' && event.isPrimary) {
        gesture = { id: event.pointerId, x: event.clientX, y: event.clientY };
        restart();
      }
    }, { signal });
    track.addEventListener('pointercancel', (event) => {
      if (event.pointerId !== gesture?.id) return;
      gesture = undefined;
      restart();
    }, { signal });
    track.addEventListener('pointerup', (event) => {
      if (event.pointerId !== gesture?.id) return;
      const x = event.clientX - gesture.x;
      const y = event.clientY - gesture.y;
      gesture = undefined;
      if (Math.abs(x) < 42 || Math.abs(x) <= Math.abs(y)) {
        restart();
        return;
      }
      ignoreClickUntil = performance.now() + 500;
      const direction = Math.sign(x);
      show(current + direction * forward, direction, true);
    }, { signal });
    track.addEventListener('click', (event) => {
      if (event.detail > 0 && performance.now() < ignoreClickUntil) event.preventDefault();
    }, { signal });

    this.stop = () => {
      listeners.abort();
      clearTimeout(timer);
      settle();
    };
    controls.hidden = false;
    settle();
    indicators.forEach((indicator, i) => {
      if (i === current) indicator.setAttribute('aria-current', 'true');
      else indicator.removeAttribute('aria-current');
    });
    restart();
  }

  disconnectedCallback() {
    this.stop?.();
    this.stop = undefined;
  }
});
