// GoldEx Smart Showcase Tray - Hardware Barcode Wedge Scanner Listener
(function () {
    let dotNetHelper = null;
    let buffer = '';
    let lastKeyTime = 0;
    const MAX_KEY_INTERVAL_MS = 50; // Barcode scanners type with < 40ms interval

    function normalizeDigits(str) {
        if (!str) return '';
        const persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
        const arabicDigits = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
        let result = str;
        for (let i = 0; i < 10; i++) {
            result = result.replaceAll(persianDigits[i], i.toString());
            result = result.replaceAll(arabicDigits[i], i.toString());
        }
        return result;
    }

    function handleKeyDown(e) {
        if (!dotNetHelper) return;

        // Ignore modifier keys
        if (e.ctrlKey || e.altKey || e.metaKey) return;

        const activeTag = document.activeElement ? document.activeElement.tagName.toLowerCase() : '';
        const isInputField = activeTag === 'input' || activeTag === 'textarea';

        // If user is focused on any text field, let standard Blazor input handling manage it
        if (isInputField) {
            return;
        }

        const now = Date.now();
        const interval = now - lastKeyTime;
        lastKeyTime = now;

        if (e.key === 'Enter') {
            if (buffer.length >= 3) {
                const cleaned = normalizeDigits(buffer.trim());
                if (cleaned.length >= 3) {
                    e.preventDefault();
                    dotNetHelper.invokeMethodAsync('OnHardwareBarcodeScanned', cleaned);
                }
            }
            buffer = '';
            return;
        }

        // Only append single printable characters
        if (e.key && e.key.length === 1) {
            // If interval is larger than scanner threshold and we're not inside a barcode input, reset buffer
            if (interval > 300 && buffer.length > 0) {
                buffer = '';
            }
            buffer += e.key;
        }
    }

    window.GoldExSmartTrayScanner = {
        initialize: function (dotNetRef) {
            dotNetHelper = dotNetRef;
            buffer = '';
            lastKeyTime = 0;
            window.removeEventListener('keydown', handleKeyDown);
            window.addEventListener('keydown', handleKeyDown);
        },

        dispose: function () {
            window.removeEventListener('keydown', handleKeyDown);
            dotNetHelper = null;
            buffer = '';
        },

        getVideoDevices: async function () {
            try {
                if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
                    console.warn('[GoldEx Camera] navigator.mediaDevices is not available (insecure context or unsupported browser).');
                    return [];
                }

                try {
                    const stream = await navigator.mediaDevices.getUserMedia({
                        video: { facingMode: { ideal: "environment" } }
                    });
                    if (stream) {
                        stream.getTracks().forEach(track => track.stop());
                    }
                } catch (permErr) {
                    console.debug('[GoldEx Camera] Permission prompt:', permErr);
                }

                const devices = await navigator.mediaDevices.enumerateDevices();
                return devices
                    .filter(device => device.kind === 'videoinput')
                    .map(device => ({
                        deviceId: device.deviceId,
                        label: device.label || 'دوربین'
                    }));
            } catch (err) {
                console.warn('[GoldEx Camera] Enumeration error:', err);
                return [];
            }
        }
    };

    window.GoldEx = window.GoldEx || {};
    window.GoldEx.getVideoDevices = window.GoldExSmartTrayScanner.getVideoDevices;
})();
