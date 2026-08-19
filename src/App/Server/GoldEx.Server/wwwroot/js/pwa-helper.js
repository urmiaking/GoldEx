// PWA Helper Script for GoldEx
// Handles service worker registration, background updates, and platform-specific installation prompt UI.

(function () {
  // Dynamic CSS Styling Injection for PWA Toasts
  const style = document.createElement('style');
  style.textContent = `
    .pwa-toast {
      position: fixed;
      bottom: 96px; /* Safe space above bottom MobileNav on mobile/tablet */
      left: 24px;
      right: 24px;
      max-width: 420px;
      background-color: rgba(var(--mud-palette-surface-rgb, 24, 24, 27), 0.88);
      backdrop-filter: blur(14px) saturate(130%);
      -webkit-backdrop-filter: blur(14px) saturate(130%);
      border: 1px solid rgba(var(--mud-palette-primary-rgb, 218, 165, 32), 0.35);
      border-radius: 16px;
      padding: 18px;
      color: var(--mud-palette-text-primary, #f8fafc);
      box-shadow: 0 12px 30px -5px rgba(0, 0, 0, 0.5), 0 8px 10px -6px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.12);
      z-index: 10000;
      font-family: var(--mud-font-family, inherit);
      direction: rtl;
      display: flex;
      flex-direction: column;
      gap: 12px;
      animation: pwa-slide-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
    @media (min-width: 576px) {
      .pwa-toast {
        left: 24px;
        right: auto;
        width: 380px;
      }
    }
    @media (min-width: 768px) {
      .pwa-toast {
        bottom: 24px; /* Lower position on desktop */
      }
    }
    @keyframes pwa-slide-in {
      from {
        transform: translateY(100px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }
    .pwa-toast-title {
      font-weight: 700;
      font-size: 15.5px;
      margin: 0;
      color: var(--mud-palette-text-primary, #ffffff);
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .pwa-toast-body {
      font-size: 13px;
      line-height: 1.6;
      margin: 0;
      color: var(--mud-palette-text-secondary, #cbd5e1);
    }
    .pwa-toast-actions {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      margin-top: 4px;
    }
    .pwa-btn {
      padding: 7px 16px;
      font-size: 12.5px;
      font-weight: 600;
      border-radius: 8px;
      cursor: pointer;
      border: none;
      transition: all 0.2s ease-in-out;
      font-family: inherit;
    }
    .pwa-btn-primary {
      background: var(--mud-palette-primary, #daa520);
      color: var(--mud-palette-primary-text, #ffffff);
    }
    .pwa-btn-primary:hover {
      background: var(--mud-palette-primary-darken, #b8860b);
      filter: brightness(0.95);
    }
    .pwa-btn-secondary {
      background: rgba(255, 255, 255, 0.1);
      color: var(--mud-palette-text-secondary, #cbd5e1);
    }
    .pwa-btn-secondary:hover {
      background: rgba(255, 255, 255, 0.18);
      color: var(--mud-palette-text-primary, #ffffff);
    }
  `;
  document.head.appendChild(style);

  // Global variables for compatibility
  window.deferredInstallPrompt = null;
  window.canInstallPwa = false;

  // Service Worker Registration
  if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
      navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' })
        .then(registration => {
          // Listen for updatefound event to detect service worker updates
          registration.addEventListener('updatefound', () => {
            const newWorker = registration.installing;
            if (newWorker) {
              newWorker.addEventListener('statechange', () => {
                if (newWorker.state === 'installed') {
                  if (navigator.serviceWorker.controller) {
                    // There is an existing active service worker; show update toast
                    showUpdateToast(newWorker);
                  }
                }
              });
            }
          });
        })
        .catch(error => {
          console.error('Service Worker registration failed:', error);
        });
    });

    // Reload the page once the new service worker activated and took control
    let refreshing = false;
    navigator.serviceWorker.addEventListener('controllerchange', () => {
      if (!refreshing) {
        refreshing = true;
        window.location.reload();
      }
    });
  }

  // Toast for background update installation
  function showUpdateToast(worker) {
    if (document.getElementById('pwa-update-toast')) return;

    const toast = document.createElement('div');
    toast.id = 'pwa-update-toast';
    toast.className = 'pwa-toast';
    toast.innerHTML = `
      <h4 class="pwa-toast-title">بروزرسانی برنامه</h4>
      <p class="pwa-toast-body">نسخه جدیدی از برنامه در حال نصب در پس‌زمینه است. برنامه به طور خودکار بارگذاری مجدد خواهد شد...</p>
    `;
    document.body.appendChild(toast);

    // Give user 2.5 seconds to read the toast, then post skipWaiting to activate update
    setTimeout(() => {
      worker.postMessage({ action: 'skipWaiting' });
    }, 2500);
  }

  // Installation Prompt for Android/Windows/Mac/Linux
  window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    window.deferredInstallPrompt = e;
    window.canInstallPwa = true;

    // Check if user has already dismissed it previously
    if (localStorage.getItem('pwa_install_dismissed') === 'true') {
      return;
    }

    // Show the install banner
    showInstallBanner();
  });

  function showInstallBanner() {
    if (document.getElementById('pwa-install-toast')) return;

    const toast = document.createElement('div');
    toast.id = 'pwa-install-toast';
    toast.className = 'pwa-toast';
    toast.innerHTML = `
      <h4 class="pwa-toast-title">نصب اپلیکیشن</h4>
      <p class="pwa-toast-body">آیا می‌خواهید این برنامه را جهت دسترسی سریع‌تر روی دستگاه خود نصب کنید؟</p>
      <div class="pwa-toast-actions">
        <button class="pwa-btn pwa-btn-secondary" id="pwa-btn-later">بعداً</button>
        <button class="pwa-btn pwa-btn-primary" id="pwa-btn-install">نصب</button>
      </div>
    `;
    document.body.appendChild(toast);

    document.getElementById('pwa-btn-install').addEventListener('click', () => {
      toast.remove();
      if (window.deferredInstallPrompt) {
        window.deferredInstallPrompt.prompt();
        window.deferredInstallPrompt.userChoice.then((choiceResult) => {
          if (choiceResult.outcome === 'accepted') {
            console.log('User accepted the PWA install prompt');
            localStorage.setItem('pwa_install_dismissed', 'true');
          } else {
            console.log('User dismissed the PWA install prompt');
          }
          window.deferredInstallPrompt = null;
          window.canInstallPwa = false;
        });
      }
    });

    document.getElementById('pwa-btn-later').addEventListener('click', () => {
      toast.remove();
      localStorage.setItem('pwa_install_dismissed', 'true');
    });
  }

  // Installation prompt for iOS Safari
  window.addEventListener('load', () => {
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true;

    if (isIOS && !isStandalone) {
      if (localStorage.getItem('pwa_ios_install_dismissed') !== 'true') {
        // Show instructions after page is fully loaded
        setTimeout(showIOSInstallBanner, 3000);
      }
    }
  });

  function showIOSInstallBanner() {
    if (document.getElementById('pwa-ios-install-toast')) return;

    const toast = document.createElement('div');
    toast.id = 'pwa-ios-install-toast';
    toast.className = 'pwa-toast';
    toast.innerHTML = `
      <h4 class="pwa-toast-title">نصب در آیفون</h4>
      <p class="pwa-toast-body">برای نصب این برنامه روی آیفون خود، روی دکمه اشتراک‌گذاری (Share) در پایین مرورگر بزنید و سپس گزینه «Add to Home Screen» (افزودن به صفحه اصلی) را انتخاب کنید.</p>
      <div class="pwa-toast-actions">
        <button class="pwa-btn pwa-btn-primary" id="pwa-btn-ios-ok">باشه</button>
      </div>
    `;
    document.body.appendChild(toast);

    document.getElementById('pwa-btn-ios-ok').addEventListener('click', () => {
      toast.remove();
      localStorage.setItem('pwa_ios_install_dismissed', 'true');
    });
  }

  // Compatibility functions
  window.getPwaState = function () {
    return !!window.canInstallPwa;
  };

  window.installPwa = async function () {
    if (!window.deferredInstallPrompt) {
      return false;
    }
    const promptEvent = window.deferredInstallPrompt;
    window.deferredInstallPrompt = null;
    window.canInstallPwa = false;

    await promptEvent.prompt();
    const result = await promptEvent.userChoice;
    return result.outcome === 'accepted';
  };

  window.isPwaMode = function () {
    if (window.navigator.standalone === true) return true;
    if (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) return true;
    if (window.matchMedia && window.matchMedia('(display-mode: fullscreen)').matches) return true;
    if (window.matchMedia && window.matchMedia('(display-mode: minimal-ui)').matches) return true;
    return false;
  };

  window.swEvents = {
    startListening: function () {
      // Retained for backward compatibility
    }
  };
})();
