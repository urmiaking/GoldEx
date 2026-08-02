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

window.printAndClose = function () {
    const splashElements = document.querySelectorAll('#app-loading, #loading-indicator, .loading-container, .splash-screen, .loading-message');
    splashElements.forEach(function (el) {
        try { el.remove(); } catch (e) {}
    });

    window.focus();
    window.print();
    window.onafterprint = function () {
        try { window.close(); } catch (e) {}
    };
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