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
            drawPill(ctx, 775, 265, 190, 52, "#181b24", "#f3f4f6", "وزن: " + data.weight + " گرم");
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
          ctx.fillText("محاسبه زنده بر اساس مظنه روز طلا", 540, priceBoxY + 45);

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
  shareProduct: async function (data) {
    window.goldexVitrine._currentShareData = data;
    var btn = document.getElementById("vitrineShareBtn");
    var origText = "";
    if (btn) {
      origText = btn.innerHTML;
      btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> <span>در حال آماده‌سازی استوری...</span>';
      btn.disabled = true;
    }

    try {
      var blob = await window.goldexVitrine.generateStoryCardBlob(data);
      window.goldexVitrine._currentStoryBlob = blob;

      var file = new File([blob], "story-" + (data.barcode || "gold") + ".png", { type: "image/png" });
      var sharePayload = {
        title: data.productName || "طلا و جواهر",
        text: (data.productName ? data.productName + " - " : "") + (data.priceFormatted ? ("قیمت: " + data.priceFormatted + " تومان - ") : "") + "مشاهده در ویترین: " + data.url,
        url: data.url,
        files: [file]
      };

      // If mobile supports sharing files directly to Instagram Stories / WhatsApp / Telegram
      if (navigator.canShare && navigator.canShare({ files: [file] })) {
        if (btn) {
          btn.innerHTML = origText;
          btn.disabled = false;
        }
        await navigator.share(sharePayload);
        return;
      }
    } catch (err) {
      console.log("Native share cancelled or not available, showing modal:", err);
    } finally {
      if (btn) {
        btn.innerHTML = origText;
        btn.disabled = false;
      }
    }

    // Fallback: Open Luxury Share Sheet Modal
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

  // Open Multi-App Share Bottom Sheet Modal
  openShareModal: function (data) {
    data = data || window.goldexVitrine._currentShareData;
    if (!data) return;

    var existing = document.getElementById("vitrineShareModal");
    if (existing) existing.remove();

    var encodedUrl = encodeURIComponent(data.url || window.location.href);
    var shareMsg = (data.productName ? data.productName + "\n" : "") +
                   (data.priceFormatted ? "قیمت روز: " + data.priceFormatted + " تومان\n" : "") +
                   (data.weight ? "وزن: " + data.weight + " گرم - عیار " + (data.fineness || "750") + "\n" : "") +
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
              <i class="fa-solid fa-share-nodes" style="color: var(--accent-color); font-size: 1.25rem;"></i>
              <h3 style="margin: 0; font-size: 1.15rem; font-weight: 800; color: #fff;">اشتراک‌گذاری کالا</h3>
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
                <div style="font-weight: 800; font-size: 0.98rem; color: #fff;">دانلود کارت آماده استوری اینستاگرام</div>
                <div style="font-size: 0.8rem; color: var(--gray-400);">طراحی شیک عمودی (۹:۱۶) همراه با عکس و مشخصات کامل</div>
              </div>
              <i class="fa-solid fa-download" style="color: var(--accent-color); font-size: 1.1rem;"></i>
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
            <input type="text" readonly value="${data.url || window.location.href}" class="vitrine-share-link-input" />
            <button type="button" class="btn-gisu btn-gisu-accent" style="padding: 10px 18px; font-size: 0.88rem;" onclick="window.goldexVitrine.copyShareLink('${data.url}')">
              <i class="fa-solid fa-copy"></i>
              <span>کپی لینک</span>
            </button>
          </div>
        </div>
      </div>
    `;

    document.body.insertAdjacentHTML("beforeend", modalHtml);
  },

  closeShareModal: function () {
    var modal = document.getElementById("vitrineShareModal");
    if (modal) {
      modal.classList.add("fade-out");
      setTimeout(function () { modal.remove(); }, 250);
    }
  }
};
