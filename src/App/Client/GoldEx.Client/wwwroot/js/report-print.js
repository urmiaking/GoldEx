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

// Localize only rendered report text. Financial values, identifiers, links and
// API data remain unchanged, including their underlying machine-readable values.
window.goldExReportToPersianDigits = function (value) {
    const persianDigits = '۰۱۲۳۴۵۶۷۸۹';
    return String(value).replace(/[0-9\u0660-\u0669]/g, char => {
        const index = char.charCodeAt(0) - (char <= '9' ? 48 : 0x0660);
        return persianDigits[index];
    });
};

window.localizeGoldExReportDigits = function (root) {
    const report = root || document.querySelector('.gex-print-document');
    if (!report) return;

    const walker = document.createTreeWalker(report, NodeFilter.SHOW_TEXT);
    let node;
    while ((node = walker.nextNode())) {
        // Never rewrite scripts or styles accidentally embedded in the report.
        if (node.parentElement?.closest('script,style,noscript,svg')) continue;
        if (/[0-9\u0660-\u0669]/.test(node.nodeValue)) {
            node.nodeValue = window.goldExReportToPersianDigits(node.nodeValue);
        }
    }
};

window.printAndClose = async function () {
    if (window.__goldexReportPrintStarted) return;
    window.__goldexReportPrintStarted = true;
    window.localizeGoldExReportDigits();

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