window.GoldEx = window.GoldEx || {};
window.GoldEx.removeSplash = function () {
    var el = document.getElementById('app-loading');
    if (el) {
        el.classList.add('fade-out');
        setTimeout(function () { el.remove(); }, 300);
    }
};

window.removeLoadingIndicator = () => {
    const el = document.getElementById('loading-indicator');

    if (el) {
        el.remove();

    }
};

document.addEventListener('click', event => {
    if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey)
        return;

    const link = event.target.closest?.('a[data-invoice-print-popup]');
    if (!link || link.origin !== window.location.origin)
        return;

    const width = Math.min(1060, screen.availWidth);
    const height = Math.min(840, screen.availHeight);
    const left = Math.round(window.screenX + (window.outerWidth - width) / 2);
    const top = Math.round(window.screenY + (window.outerHeight - height) / 2);
    const popup = window.open('about:blank', '_blank', `popup,scrollbars=yes,resizable=yes,width=${width},height=${height},left=${left},top=${top}`);
    if (!popup)
        return;

    popup.opener = null;
    popup.location.replace(link.href);
    event.preventDefault();
});
