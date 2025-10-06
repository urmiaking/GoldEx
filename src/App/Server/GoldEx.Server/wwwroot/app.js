window.removeLoadingIndicator = () => {
    const el = document.getElementById('loading-indicator');

    if (el) {
        el.remove();

    }
};