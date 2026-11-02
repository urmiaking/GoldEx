// Native invoice table scrolls with the document, below the sticky filter toolbar.
// Measure filter height when active filter chips wrap or responsive sizes change.
export function trackFilterHeight(directory, filters) {
    if (!directory || !filters) return { dispose() {} };

    let disposed = false;
    const update = () => {
        if (disposed) return;
        const style = getComputedStyle(filters);
        const margin = Number.parseFloat(style.marginBottom) || 0;
        const height = Math.ceil(filters.getBoundingClientRect().height + margin);
        directory.style.setProperty('--invoice-filters-block-size', `${height}px`);
    };

    const resizeObserver = new ResizeObserver(update);
    resizeObserver.observe(filters);
    update();

    return {
        dispose() {
            if (disposed) return;
            disposed = true;
            resizeObserver.disconnect();
            directory.style.removeProperty('--invoice-filters-block-size');
        }
    };
}
