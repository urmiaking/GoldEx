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