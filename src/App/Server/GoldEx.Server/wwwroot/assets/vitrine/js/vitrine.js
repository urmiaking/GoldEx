/* ==========================================================================
   GoldEx Vitrine - Interactive Helpers & Image Gallery
   ========================================================================== */

window.goldexVitrine = {
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

  copyShareLink: function (title) {
    var url = window.location.href;
    if (navigator.clipboard) {
      navigator.clipboard.writeText(url).then(function () {
        window.goldexVitrine.showToast("لینک محصول کپی شد.");
      });
    } else {
      var temp = document.createElement("input");
      document.body.appendChild(temp);
      temp.value = url;
      temp.select();
      document.execCommand("copy");
      document.body.removeChild(temp);
      window.goldexVitrine.showToast("لینک محصول کپی شد.");
    }
  },

  showToast: function (msg) {
    var existing = document.getElementById("vitrineToast");
    if (existing) existing.remove();

    var toast = document.createElement("div");
    toast.id = "vitrineToast";
    toast.style.position = "fixed";
    toast.style.bottom = "30px";
    toast.style.left = "50%";
    toast.style.transform = "translateX(-50%)";
    toast.style.background = "linear-gradient(135deg, #e0b64a, #9e741a)";
    toast.style.color = "#08090d";
    toast.style.padding = "14px 28px";
    toast.style.borderRadius = "999px";
    toast.style.fontWeight = "900";
    toast.style.boxShadow = "0 10px 35px rgba(0,0,0,0.6)";
    toast.style.zIndex = "9999";
    toast.style.direction = "rtl";
    toast.style.fontSize = "0.95rem";
    toast.innerText = msg;

    document.body.appendChild(toast);
    setTimeout(function () {
      toast.style.opacity = "0";
      toast.style.transition = "opacity 0.4s ease";
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
  }
};
