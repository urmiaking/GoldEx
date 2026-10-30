window.openReportPopup = function (url) {
    const width = 1000;
    const height = 800;

    const left = (screen.width - width) / 2;
    const top = (screen.height - height) / 2;

    window.open(
        url,
        '_blank',
        `toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,resizable=yes,width=${width},height=${height},top=${top},left=${left}`
    );
};

window.printAndClose = async function () {
    if (window.__goldexReportPrintStarted) return;
    window.__goldexReportPrintStarted = true;

    // Report data can arrive before its fonts, stylesheet, or store logo.
    const splash = document.querySelectorAll(
        '#app-loading, #loading-indicator, .loading-container, .splash-screen, .loading-message'
    );
    splash.forEach(element => element.remove());

    const withTimeout = (promise, ms = 4000) =>
        Promise.race([promise, new Promise(resolve => setTimeout(resolve, ms))]);
    try {
        await withTimeout(Promise.all(Array.from(document.querySelectorAll('link[rel="stylesheet"]')).map(link => {
            if (link.sheet) return Promise.resolve();
            return new Promise(resolve => {
                link.addEventListener('load', resolve, { once: true });
                link.addEventListener('error', resolve, { once: true });
            });
        })));
        if (document.fonts?.ready) await withTimeout(document.fonts.ready);
        await withTimeout(Promise.all(Array.from(document.images).map(image => {
            if (image.complete) return Promise.resolve();
            return new Promise(resolve => {
                image.addEventListener('load', resolve, { once: true });
                image.addEventListener('error', resolve, { once: true });
            });
        })));
        await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    } catch (_) {
        // Non-essential assets must not make printing impossible.
    }

    window.addEventListener('afterprint', function () {
        try { window.close(); } catch (_) { /* non-popup tabs stay open */ }
    }, { once: true });
    window.focus();
    window.print();
};

//window.printAndClose = function () {
//    window.focus();

//    window.print();

//    setTimeout(() => {
//        try {
//            window.close();
//        } catch {
//            // ignored
//        }
//    }, 500);
//};