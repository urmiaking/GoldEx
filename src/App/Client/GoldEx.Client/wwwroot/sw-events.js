window.swEvents = {
    startListening: function () {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('service-worker.js')
                .then(reg => {
                    reg.addEventListener('updatefound', () => {
                        const installingWorker = reg.installing;
                        if (installingWorker) {
                            DotNet.invokeMethodAsync('GoldEx.Client.Components', 'OnServiceWorkerInstalling');
                        }
                    });

                    navigator.serviceWorker.addEventListener('message', event => {
                        if (event.data?.type === 'NEW_VERSION_AVAILABLE') {
                            window.location.reload();
                        }
                    });
                });
        }

        // ---- PWA install state ----
        window.deferredInstallPrompt = null;
        window.canInstallPwa = false;

        window.addEventListener('beforeinstallprompt', function (e) {
            e.preventDefault();
            window.deferredInstallPrompt = e;
            window.canInstallPwa = true;
        });
    }
};

// آیا الان می‌تونیم نصب کنیم؟
window.getPwaState = function () {
    return !!window.canInstallPwa;
};

// اجرا کردن نصب
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

// چک کردن اینکه همین پنجره PWA هست یا نه
window.isPwaMode = function () {
    // iOS
    if (window.navigator.standalone === true) {
        return true;
    }

    // سایر مرورگرها
    if (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) {
        return true;
    }
    if (window.matchMedia && window.matchMedia('(display-mode: fullscreen)').matches) {
        return true;
    }
    if (window.matchMedia && window.matchMedia('(display-mode: minimal-ui)').matches) {
        return true;
    }

    return false;
};